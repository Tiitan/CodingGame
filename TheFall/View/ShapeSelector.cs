using System.Windows;
using System.Windows.Controls;
using TheFall.ViewModel;

namespace TheFall.View
{
    internal class ShapeSelector : DataTemplateSelector
    {
        public DataTemplate? Empty { get; set; }
        public DataTemplate? CrossShape { get; set; }
        public DataTemplate? LineShape { get; set; }
        public DataTemplate? DoubleCurveShape { get; set; }
        public DataTemplate? TShape { get; set; }
        public DataTemplate? CurveSHape { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CellViewModel cellViewModel = (CellViewModel)item;

            var templates = new DataTemplate[] {
                Empty!,
                CrossShape!,
                LineShape!, LineShape!,
                DoubleCurveShape!, DoubleCurveShape!,
                TShape!, TShape!, TShape!, TShape!,
                CurveSHape!, CurveSHape!, CurveSHape!, CurveSHape!
            };

            return templates[cellViewModel.Type];
        }
    }
}
