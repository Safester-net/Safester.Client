using System;
using System.ComponentModel;
using Dripdoctors.iOS;
using Safester.Controls;
using Safester.Utils;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedViewCell), typeof(ExtendedViewCellRenderer))]
namespace Dripdoctors.iOS
{
    public class ExtendedViewCellRenderer : ViewCellRenderer
    {
        private UITableViewCell _cell { get; set; }

        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);
            var view = item as ExtendedViewCell;
            cell.SelectedBackgroundView = new UIView
            {
                BackgroundColor = ThemeHelper.GetMenuSelectionColor().ToUIColor(),
            };

            view.SelectionChanged += () =>
            {
                if (_cell != null && _cell.SelectedBackgroundView != null)
                {
                    _cell.SelectedBackgroundView.BackgroundColor = ThemeHelper.GetMenuSelectionColor().ToUIColor();
                }
            };

            _cell = cell;
            return cell;
        }

        
    }
}