using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace FlowSimulator.AvalonEdit
{
	public class BraceFoldingStrategy : AbstractFoldingStrategy
	{
		public char OpeningBrace { get; set; }
		
		public char ClosingBrace { get; set; }

        public string BeginRegion { get; set; }

        public string EndRegion { get; set; }
		
		public BraceFoldingStrategy()
		{
			OpeningBrace = '{';
			ClosingBrace = '}';
            BeginRegion = "#region";
            EndRegion = "#endregion";
		}
		
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}
		
		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
		{
			List<NewFolding> newFoldings = new List<NewFolding>();
			
			Stack<int> startOffsets = new Stack<int>();
            Stack<int> startRegionOffsets = new Stack<int>();

			int lastNewLineOffset = 0;
			char openingBrace = OpeningBrace;
			char closingBrace = ClosingBrace;

			for (int i = 0; i < document.TextLength; i++) 
            {
                char c = document.GetCharAt(i);

                if (i + BeginRegion.Length < document.TextLength
                    && BeginRegion.Equals(document.GetText(i, BeginRegion.Length)))
                {
                    startRegionOffsets.Push(i);
                }
                else if (i + EndRegion.Length < document.TextLength
                    && EndRegion.Equals(document.GetText(i, EndRegion.Length)) 
                    && startRegionOffsets.Count > 0) 
                {
                    int startOffset = startRegionOffsets.Pop();
                    int endOffset = i + 1;

                    while (startOffset < document.TextLength
                        &&
                            (document.GetCharAt(startOffset) != '\n'
                            && document.GetCharAt(startOffset) != '\r'))
                    {
                        startOffset++;
                    }

                    while (endOffset < document.TextLength
                        &&
                            (document.GetCharAt(endOffset) != '\n'
                            && document.GetCharAt(endOffset) != '\r'))
                    {
                        endOffset++;
                    }

                    {
                        newFoldings.Add(new NewFolding(startOffset, endOffset));
                    }
                }
                else if (c == openingBrace) 
                {
					startOffsets.Push(i);
				} 
                else if (c == closingBrace && startOffsets.Count > 0) 
                {
					int startOffset = startOffsets.Pop();
					if (startOffset < lastNewLineOffset) {
						newFoldings.Add(new NewFolding(startOffset, i + 1));
					}
				} 
                else if (c == '\n' || c == '\r') 
                {
					lastNewLineOffset = i + 1;
				}
			}
			newFoldings.Sort((a,b) => a.StartOffset.CompareTo(b.StartOffset));
			return newFoldings;
		}
	}
}
