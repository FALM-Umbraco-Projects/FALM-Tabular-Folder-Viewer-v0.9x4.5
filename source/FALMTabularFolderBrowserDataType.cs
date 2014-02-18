using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using umbraco.cms.businesslogic.datatype;

namespace FALMTabularFolderBrowser
{
	public class FALMTabularFolderBrowserDataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
	{
		private umbraco.interfaces.IDataEditor _Editor;
		private umbraco.interfaces.IData _baseData;
		private FALMTabularFolderBrowserPrevalueEditor _prevalueeditor;

		public override umbraco.interfaces.IDataEditor DataEditor
		{
			get
			{
				if (_Editor == null)
					_Editor = new FALMTabularFolderBrowserDataEditor(Data, ((FALMTabularFolderBrowserPrevalueEditor) PrevalueEditor).Configuration);
				return _Editor;
			}
		}

		public override umbraco.interfaces.IData Data
		{
			get
			{
				if (_baseData == null)
					_baseData = new DefaultData(this);
				return _baseData;
			}
		}
		public override Guid Id
		{
			get
			{
				return new Guid("c0e41dd5-c9a3-4851-8546-a9fe81d97acb");
			}
		}

		public override string DataTypeName
		{
			get
			{
				return "FALM Tabular Folder Browser";
			}
		}

		public override umbraco.interfaces.IDataPrevalue PrevalueEditor
		{
			get
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new FALMTabularFolderBrowserPrevalueEditor(this);
				return _prevalueeditor;
			}
		}
	}
}
