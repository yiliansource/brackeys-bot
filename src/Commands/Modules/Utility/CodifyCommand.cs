using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.Commands;

using BrackeysBot.Services;

namespace BrackeysBot.Commands
{
	public partial class UtilityModule : BrackeysBotModule
	{
		[Command("codify"), Alias("code")]
		[Summary("Turns the pasted code into a beautifully formatted code block.")]
		[Remarks("code <input>")]
		public async Task CodifyCommandAsync([Summary("Your copy pasted code."), Remainder] string input)
		{
			await Context.Message.DeleteAsync();

			if (FilterService.ContainsBlockedWord(input))
			{
				return;
			}

			var prefix = "```cs\n";
			var suffix = "\n```";
			var codeLines = input.Split('\n').ToList();

			RemoveEmptyMethods(codeLines);

			RemoveEmptyLines(codeLines);

			var alignedCodeLines = AlignToLeft(codeLines);

			var output = string.Join('\n', alignedCodeLines);
			var blockedCode = $"{prefix}{output}{suffix}";

			await Context.Channel.SendMessageAsync(blockedCode);
		}

		// Removes empty void methods like the default Update() and Start() that some people can't be bothered to delete themselves
		private static void RemoveEmptyMethods(List<string> codeLines)
		{
			var isChecking = false;
			var startIndex = 0;

			for (int lineIndex = 0; lineIndex < codeLines.Count; lineIndex++)
			{
				if (codeLines[lineIndex].Contains("// Update is called once per frame") || codeLines[lineIndex].Contains("// Start is called before the first frame update"))
				{
					codeLines.RemoveAt(lineIndex);
				}
				if (codeLines[lineIndex].Contains("void"))
				{
					isChecking = true;
					startIndex = lineIndex;
				}
				else if (isChecking && codeLines[lineIndex].Contains("}"))
				{
					isChecking = false;
					for (int i = startIndex; i < lineIndex + 1; i++)
					{
						codeLines.RemoveAt(startIndex);
					}
					startIndex = 0;
					lineIndex = 0;
				}
				else if (isChecking && !string.IsNullOrWhiteSpace(codeLines[lineIndex]) && !codeLines[lineIndex].Contains("{"))
				{
					isChecking = false;
					startIndex = 0;
				}
			}
		}

		// Removes consecutive empty lines until there is only one left
		private static void RemoveEmptyLines(List<string> codeLines)
		{
			// Removes big spaces
			for (var lineIndex = 0; lineIndex < codeLines.Count; lineIndex++)
			{
				if ((lineIndex > 0 && string.IsNullOrWhiteSpace(codeLines[lineIndex]) && string.IsNullOrWhiteSpace(codeLines[lineIndex - 1])) || (lineIndex == 0 && string.IsNullOrWhiteSpace(codeLines[lineIndex])))
				{
					codeLines.RemoveAt(lineIndex);
					lineIndex--;
				}
			}

			// Removes the space left over by removing empty methods
			for (var lineIndex = 0; lineIndex < codeLines.Count; lineIndex++)
			{
				if (codeLines[lineIndex].Contains("{") && string.IsNullOrWhiteSpace(codeLines[lineIndex + 1]) ||
					lineIndex + 2 < codeLines.Count && codeLines[lineIndex].Contains("}") && codeLines[lineIndex + 2].Contains("}") && string.IsNullOrWhiteSpace(codeLines[lineIndex + 1]))
				{
					codeLines.RemoveAt(lineIndex + 1);
				}
			}
		}

		// Aligns the code block to the left as much as it can without changing the internal relative indentation
		private static string[] AlignToLeft(List<string> codeLines)
		{
			var spacesToTrim = int.MaxValue;
			for (int lineIndex = 1; lineIndex < codeLines.Count; lineIndex++)   // Finds the minimum indentation to calculate spaces to trim
			{
				for (int charIndex = 0; charIndex < codeLines[lineIndex].Length; charIndex++)
				{
					if (codeLines[lineIndex][charIndex] != ' ')
					{
						if (spacesToTrim > charIndex)
						{
							spacesToTrim = charIndex;
						}
					}
				}
			}

			string[] trimmedCodeLines = new string[codeLines.Count];
			for (int lineIndex = 0; lineIndex < codeLines.Count; lineIndex++)
			{
				if (lineIndex == 0 && codeLines[lineIndex][0] != ' ')
				{
					trimmedCodeLines[lineIndex] = codeLines[lineIndex];
				}
				else if (!string.IsNullOrWhiteSpace(codeLines[lineIndex]))
				{
					trimmedCodeLines[lineIndex] = codeLines[lineIndex].Substring(spacesToTrim);
				}
			}

			return trimmedCodeLines;
		}
	}
}
