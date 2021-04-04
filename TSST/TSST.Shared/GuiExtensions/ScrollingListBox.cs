using System.Windows.Controls;

namespace TSST.Shared.GuiExtensions
{
    public class ScrollingListBox : ListBox
    {
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                base.OnItemsChanged(e);
                return;
            }

            var newItemCount = e.NewItems.Count;

            if (newItemCount > 0)
                ScrollIntoView(e.NewItems[newItemCount - 1]);

            base.OnItemsChanged(e);
        }
    }
}
