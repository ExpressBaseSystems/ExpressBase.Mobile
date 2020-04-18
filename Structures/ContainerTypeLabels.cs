using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Structures
{
    public struct ContainerLabels
    {
        public const string FormsLabel = "Forms";

        public const string ListsLabel = "Lists";

        public const string DashBoardsLabel = "DashBoards";

        public const string PdfLabel = "Pdf's";

        public static IEnumerable<string> ListOrder
        {
            get
            {
                return new[]
                {
                    ListsLabel,
                    FormsLabel,
                    DashBoardsLabel,
                    PdfLabel
                };
            }
        }

        public static string GetLabel(EbMobileContainer container)
        {
            if (container is EbMobileForm)
                return FormsLabel;
            else if (container is EbMobileVisualization)
                return ListsLabel;
            else if (container is EbMobileDashBoard)
                return DashBoardsLabel;
            else if (container is EbMobilePdf)
                return PdfLabel;
            else
                return "Miscellaneous";
        }
    }
}
