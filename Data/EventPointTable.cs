using System;
using System.Linq;
using System.Collections.Generic;

using Discord;

namespace BrackeysBot
{
    /// <summary>
    /// Provides a table to store event points for users.
    /// </summary>
    public class EventPointTable : LookupTable<ulong, Dictionary<int, int>>
    {
        public override string FileName => "event-points";

        /// <summary>
        /// Returns the index of the current day (in UTC time).
        /// </summary>
        public int CurrentDayIndex => GetDayIndex(DateTime.UtcNow);
        /// <summary>
        /// Returns the number of users registered in the table.
        /// </summary>
        public int RegisteredUsers => Table.Keys.Count;

        private static readonly DateTime _dataStoreOffset = new DateTime(2000, 1, 1, 0, 0, 0);

        private const int POINT_EXPIRATION_DAYS = 30 * 4;

        /// <summary>
        /// Returns the points for the specified user, capped by the expiration days.
        /// </summary>
        public int GetPoints(IUser user)
            => GetPoints(user.Id);
        /// <summary>
        /// Returns the points for the specified user ID, capped by the expiration days.
        /// </summary>
        public int GetPoints(ulong userId)
            => Has(userId) ? GetPointsCappedByDayCount(userId) : 0;

        /// <summary>
        /// Adds a certain amount of points to a user.
        /// </summary>
        public void AddPoints(IUser user, int points)
            => AddPoints(user.Id, points);
        /// <summary>
        /// Adds a certain amount of points to a user ID.
        /// </summary>
        public void AddPoints(ulong userId, int points)
            => ModifyPoints(userId, +points);
        
        /// <summary>
        /// Removes a certain amount of points from a user.
        /// </summary>
        public void RemovePoints(IUser user, int points)
            => RemovePoints(user.Id, points);
        /// <summary>
        /// Removes a certain amount of points from a user ID.
        /// </summary>
        public void RemovePoints(ulong userId, int points)
            => ModifyPoints(userId, -points);

        /// <summary>
        /// Sets the points for a user to certain amount.
        /// </summary>
        public void SetPoints(IUser user, int points)
            => SetPoints(user.Id, points);
        /// <summary>
        /// Sets the points for a user ID to certain amount.
        /// </summary>
        public void SetPoints(ulong userId, int points)
        {
            int dayIndex = CurrentDayIndex;

            if (points < 0) points = 0;

            if (Has(userId))
            {
                var userPoints = Get(userId);
                userPoints.Clear();
                userPoints.Add(dayIndex, points);
            }
            else
            {
                Add(userId, new Dictionary<int, int>()
                {
                    { dayIndex, points }
                });
            }
        }

        private void ModifyPoints(ulong userId, int change)
        {
            int dayIndex = CurrentDayIndex;

            if (Has(userId))
            {
                var userPoints = Get(userId);
                if (userPoints.ContainsKey(dayIndex))
                {
                    int oldPoints = userPoints[dayIndex];
                    int newPoints = oldPoints + change;
                    userPoints[dayIndex] = newPoints;
                }
                else
                {
                    if (change > 0)
                    {
                        userPoints.Add(dayIndex, change);
                        SaveData();
                    }
                }
            }
            else
            {
                if (change > 0)
                {
                    Add(userId, new Dictionary<int, int>()
                    {
                        { dayIndex, change }
                    });
                }
            }
        }

        public void Reset()
        {
            Table.Clear();
            SaveData();
        }

        /// <summary>
        /// Returns the leaderboard, sorted by user points.
        /// </summary>
        public IOrderedEnumerable<KeyValuePair<ulong, int>> GetLeaderboard()
        {
            return Table.Keys
                .ToDictionary(u => u, u => GetPointsCappedByDayCount(u))
                .OrderByDescending(k => k.Value);
        }

        /// <summary>
        /// Returns the leaderboard, starting from the <paramref name="startIndex"/>, including a specific number of places.
        /// </summary>
        public IEnumerable<KeyValuePair<ulong, int>> GetLeaderboardPlaces(int startIndex, int count)
        {
            var leaderboard = GetLeaderboard();

            if (leaderboard.Count() < startIndex) return Enumerable.Empty<KeyValuePair<ulong, int>>();
            var skippedToIndex = leaderboard.Skip(startIndex);

            if (skippedToIndex.Count() < count) return skippedToIndex;

            return skippedToIndex.Take(count);
        }

        /// <summary>
        /// Cleans the point lookups of values that will no longer be used.
        /// </summary>
        public void CleanupPointLookups()
        {
            foreach (ulong userId in Table.Keys)
            {
                CleanupPointLookup(userId);
            }
        }
        /// <summary>
        /// Cleans the point lookup of a specific user of values that will no longer be used.
        /// </summary>
        public void CleanupPointLookup(ulong userId)
        {
            Set(userId, Get(userId).Where(k => (CurrentDayIndex - k.Key) < POINT_EXPIRATION_DAYS).ToDictionary(k => k.Key, k => k.Value));
        }

        private int GetPointsCappedByDayCount(ulong userId)
            => Get(userId).Where(k => (CurrentDayIndex - k.Key) < POINT_EXPIRATION_DAYS).Sum(k => k.Value);

        /// <summary>
        /// Returns the index of a day (since 1/1/2000).
        /// </summary>
        private static int GetDayIndex(DateTime day)
            => (int)Math.Floor((day - _dataStoreOffset).TotalDays);
    }
}
