using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Windows.Shared;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apontamento.Custom
{
    public class CustomSelectionController : GridSelectionController
    {
        public CustomSelectionController(SfDataGrid dataGrid) : base(dataGrid)
        {
        }

        protected override void ProcessKeyDown(KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                KeyEventArgs arguments = new KeyEventArgs(args.KeyboardDevice, args.InputSource, args.Timestamp, Key.Tab) { RoutedEvent = args.RoutedEvent };
                base.ProcessKeyDown(arguments);
                //assigning the state of Tab key Event handling to Enter key
                args.Handled = arguments.Handled;

                return;
            }


            base.ProcessKeyDown(args);
        }
    }
}

