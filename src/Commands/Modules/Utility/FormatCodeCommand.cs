using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BrackeysBot.Commands
{
	public partial class UtilityModule : BrackeysBotModule
	{
		[Command("format"), Alias("code", "codify")]
		[Summary("Turns input code into a formatted code block and pastes it into chat.")]
		[Remarks("format <input>")]
		public async Task FormatCodeAsync([Summary("The code input"), Remainder] string input)
		{
			if (FilterService.ContainsBlockedWord(input))
			{
				return;
			}

			await Context.Message.DeleteAsync();

			var trimmedCode = RemoveEmptyMethods(input);
			var formattedCode = FormatCode(trimmedCode);

			await Context.Channel.SendMessageAsync($"```cs\n{formattedCode}\n```");
		}

		// Removes empty void methods like the default Update() and Start() that some people can't be bothered to delete themselves
		private static string RemoveEmptyMethods(string remainingText)
		{
			var codeLines = remainingText.Split('\n').ToList();

			var isChecking = false;
			var startIndex = 0;
			int startComment = -1;
			int updateComment = -1;

			// Iterate over each line
			for (int lineIndex = 0; lineIndex < codeLines.Count; lineIndex++)
			{
				// Save comment line indices
				if (codeLines[lineIndex].Contains("// Start is called before the first frame update"))
				{
					startComment = lineIndex;
				}
				else if (codeLines[lineIndex].Contains("// Update is called once per frame"))
				{
					updateComment = lineIndex;
				}

				// Start checking starting from next line
				if (codeLines[lineIndex].Contains("void"))
				{
					isChecking = true;
					startIndex = lineIndex;
				}

				// Delete the method and comment lines if nothing other than declaration and body brackeys are found
				else if (isChecking && codeLines[lineIndex].Contains("}"))
				{
					isChecking = false;

					if (startComment != -1)
					{
						codeLines[startComment] = codeLines[startComment].Replace("// Start is called before the first frame update", string.Empty);
					}
					if (updateComment != -1)
					{
						codeLines[updateComment] = codeLines[updateComment].Replace("// Update is called once per frame", string.Empty);
					}

					for (int i = startIndex; i < lineIndex + 1; i++)
					{
						codeLines.RemoveAt(startIndex);
					}
					updateComment = -1;
					startComment = -1;
					startIndex = 0;
					lineIndex = 0;
				}

				// Stop checking if method isn't empty
				else if (isChecking && !string.IsNullOrWhiteSpace(codeLines[lineIndex]) && !codeLines[lineIndex].Contains("{"))
				{
					isChecking = false;
					updateComment = -1;
					startComment = -1;
					startIndex = 0;
				}
			}

			return string.Join('\n', codeLines);
		}

		// Formats the input using microsoft's code parser
		public static string FormatCode(string input)
		{
			var tree = CSharpSyntaxTree.ParseText(input);
			var node = tree.GetRoot().NormalizeWhitespace();
			return node.ToFullString();
		}
	}
}
