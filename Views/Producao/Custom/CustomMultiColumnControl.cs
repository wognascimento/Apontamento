using Apontamento.DataBase.Model;
using Syncfusion.UI.Xaml.Grid;

namespace Apontamento.Views.Producao.Custom
{
    class CustomMultiColumnControl : SfMultiColumnDropDownControl
    {
        /// <summary>
        /// Returns true if the item is displayed in the Filtered List, otherwise returns false.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// 

        protected override bool FilterRecord(object item)
        {
            var _item = item as FuncionarioAtivoModel;
            var result = _item.codfun.ToString().Contains(SearchText);
            return result;
        }
    }
}

