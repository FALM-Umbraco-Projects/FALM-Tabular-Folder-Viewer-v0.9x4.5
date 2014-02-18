using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;

using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.controls;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.uicontrols.DatePicker;

namespace FALMTabularFolderBrowser
{
	[ValidationProperty("IsValid")]
    public class FALMTabularFolderBrowserDataEditor : UpdatePanel, IDataEditor
	{
		private IData			_data;
		string					_configuration;
		private string[]		config;
		private string			controlType;
		private bool			showAllDescendants;
		private string[]		searchFields;
		private string[]		viewFields;
		private string			SQLWhereClause;
        private string          ViewName;
        private Hashtable       fieldSet;

        private SqlDataSource sqlDSBrowserProperties;
        private Panel pnlFilterProperties;
        private GridView gvBrowserProperties;
        private Button btnSearch;

		/// <summary>
		/// 
		/// </summary>
		public ISqlHelper SqlHelper
		{
			get
			{
				return Application.SqlHelper;
			}
		}
	
		public FALMTabularFolderBrowserDataEditor(umbraco.interfaces.IData Data, string Configuration)
        {
			_data = Data;
			_configuration = Configuration;

			config = _configuration.Split("|".ToCharArray());

			controlType = config[0];

			// Should we show children
			showAllDescendants = Convert.ToBoolean(config[1]);

			// Save the document alias array, removing white spaces
			searchFields = config[2].Replace(" ", "").Split(',');
            viewFields = config[3].Replace(" ", "").Split(',');
            SQLWhereClause = config[4];
            ViewName = config[5];

            fieldSet = new Hashtable();
            foreach (string field in config[6].Split(','))
            {
                string[] tmp = field.Split(':');

                fieldSet.Add(tmp[0], tmp[1]);
            }
        }

		public virtual bool TreatAsRichTextEditor
		{
			get
			{
				return false;
			}
		}

		public bool ShowLabel
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Internal logic for validation controls to detect whether or not it's valid (has to be public though) 
		/// </summary>
		/// <value>Am I valid?</value>
		public string IsValid
		{
			get
			{
				return "valid";
			}
		}

		public Control Editor
		{
			get
			{
				return this;
			}
		}

		public void Save()
		{
			
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            sqlDSBrowserProperties.ID = "sqlDSBrowserProperties";
            // Get the connectionString
            sqlDSBrowserProperties.ConnectionString = SqlHelper.ConnectionString;
            sqlDSBrowserProperties.SelectCommand = SQLCommandFiltered();
        }

		protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Set SQLDataSource
            sqlDSBrowserProperties = new SqlDataSource();

			// Add all Controls to container
            base.ContentTemplateContainer.Controls.Add(sqlDSBrowserProperties);
            base.ContentTemplateContainer.Controls.Add(FilterPanel());
			base.ContentTemplateContainer.Controls.Add(BrowserTable());
		}

        protected Panel FilterPanel()
        {
            pnlFilterProperties = new Panel();
            pnlFilterProperties.ID = "pnlFilterProperties";

            pnlFilterProperties.Controls.Add(new LiteralControl(@"<div class=""propertypane"">"));
      
            string strAlias = string.Empty;
            string strDbType = string.Empty;
            string strText = string.Empty;

			// Table Filter Cells
            foreach (string fieldDesc in searchFields)
            {
                // Retrieve DbType of the filter property
                strText = FALMTabularFolderBrowserUtility.GetPropertyText(fieldDesc);
                strAlias = FALMTabularFolderBrowserUtility.GetPropertyName(fieldDesc);
                strDbType = (string)fieldSet[strAlias];

                switch (strDbType)
                {
                    case "Date":
                        DateTimePicker dtpckDateFieldFrom = new DateTimePicker();
                        dtpckDateFieldFrom.ID = "dtpck" + strAlias + "From";
                        dtpckDateFieldFrom.ShowTime = false;

                        DateTimePicker dtpckDateFieldTo = new DateTimePicker();
                        dtpckDateFieldTo.ID = "dtpck" + strAlias + "To";
                        dtpckDateFieldTo.ShowTime = false;

                        Table dateRange = new Table();

                        dateRange.Rows.Add(new TableRow());
                        dateRange.Rows[0].Cells.Add(new TableCell());
                        dateRange.Rows[0].Cells.Add(new TableCell());
                        dateRange.Rows.Add(new TableRow());
                        dateRange.Rows[1].Cells.Add(new TableCell());
                        dateRange.Rows[1].Cells.Add(new TableCell());

                        dateRange.Rows[0].Cells[0].Text = "From: ";
                        dateRange.Rows[0].Cells[1].Controls.Add(dtpckDateFieldFrom);
                        dateRange.Rows[1].Cells[0].Text = "To: ";
                        dateRange.Rows[1].Cells[1].Controls.Add(dtpckDateFieldTo);

                        AddFilterControl(pnlFilterProperties.Controls, strText, dateRange);
                        break;

                    case "Int":
                    case "Nvarchar":
                    case "Ntext":
                    default:
                        TextBox txtbTextField = new TextBox();
                        txtbTextField.ID = "txtb" + strAlias;
                        txtbTextField.Width = Unit.Pixel(300);

                        AddFilterControl(pnlFilterProperties.Controls, strText, txtbTextField);
                        break;
                }
            }

            // Set Button btnSearch
            btnSearch = new Button();
            btnSearch.ID = "btnSearch";
            btnSearch.Command += new CommandEventHandler(btnSearch_Click);
            btnSearch.Text = "Search";            
            AddFilterControl(pnlFilterProperties.Controls, " ", btnSearch);

            pnlFilterProperties.Controls.Add(new LiteralControl(@"<div class=""propertyPaneFooter"">-</div></div>"));
            return pnlFilterProperties;
        }

        private void AddFilterControl(ControlCollection controls,  string name, Control ctrl)
        {
            controls.Add(new LiteralControl(@"<div class=""propertyItem"">"));
            controls.Add(new LiteralControl(@"<div class=""propertyItemheader"">"+name+"</div>"));
            controls.Add(new LiteralControl(@"<div class=""propertyItemContent"">"));
            controls.Add(ctrl);
            controls.Add(new LiteralControl(@"</div></div>"));
        }

		protected GridView BrowserTable()
		{
			// Set GridView gvBrowserProperties
			gvBrowserProperties = new GridView();
			gvBrowserProperties.ID = "gvBrowserProperties";
            gvBrowserProperties.DataSourceID = "sqlDSBrowserProperties";
            gvBrowserProperties.AllowPaging = true;
            gvBrowserProperties.AllowSorting = true;

            gvBrowserProperties.AutoGenerateColumns = false;
            foreach (string fieldDesc in viewFields)
            {
                string k = FALMTabularFolderBrowserUtility.GetPropertyName(fieldDesc);
                string w = FALMTabularFolderBrowserUtility.GetFieldWidth(fieldDesc);
                string t = FALMTabularFolderBrowserUtility.GetPropertyText(fieldDesc);

                BoundField bf = new BoundField();

                bf.HeaderText = t;
                bf.HeaderStyle.Width = Unit.Parse(w);
                bf.DataField = k;
                bf.SortExpression = k;

                gvBrowserProperties.Columns.Add(bf);
            }

            HyperLinkField hf = new HyperLinkField();
            hf.DataNavigateUrlFields = "EditID".Split(',');
            hf.DataNavigateUrlFormatString = "editContent.aspx?id={0}";
            hf.Text = "<img src=\"images/forward.png\" />";
            hf.HeaderStyle.Width = Unit.Parse("18px");
            hf.ControlStyle.Width = Unit.Parse("18px");
            hf.ItemStyle.Width = Unit.Parse("18px");
            hf.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            gvBrowserProperties.Columns.Add(hf);

            gvBrowserProperties.Width = Unit.Parse("100%");
            gvBrowserProperties.CellPadding = 4;
            gvBrowserProperties.ForeColor = System.Drawing.ColorTranslator.FromHtml("#000000");
            gvBrowserProperties.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            gvBrowserProperties.BorderColor = System.Drawing.ColorTranslator.FromHtml("#DEDFDE");
            gvBrowserProperties.BorderWidth = Unit.Parse("1px");

            gvBrowserProperties.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#6B696B");
            gvBrowserProperties.HeaderStyle.Font.Bold = true;
            gvBrowserProperties.HeaderStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            gvBrowserProperties.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;

            gvBrowserProperties.RowStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#F7F7DE");
            gvBrowserProperties.RowStyle.VerticalAlign = VerticalAlign.Middle;
            gvBrowserProperties.AlternatingRowStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF"); ;
            gvBrowserProperties.AlternatingRowStyle.VerticalAlign = VerticalAlign.Middle;

            gvBrowserProperties.PageSize = 10;

            gvBrowserProperties.PagerSettings.Mode = PagerButtons.NumericFirstLast;
            gvBrowserProperties.PagerSettings.FirstPageText = "<<";
            gvBrowserProperties.PagerSettings.LastPageText = ">>";
            gvBrowserProperties.PagerSettings.NextPageText = ">";
            gvBrowserProperties.PagerSettings.PreviousPageText = "<";
            gvBrowserProperties.PagerSettings.Position = PagerPosition.Bottom;
            gvBrowserProperties.PagerSettings.PageButtonCount = 10;
            gvBrowserProperties.PagerStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#6B696B");
            gvBrowserProperties.PagerStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            gvBrowserProperties.PagerStyle.HorizontalAlign = HorizontalAlign.Right;

            gvBrowserProperties.FooterStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#CCCC99");

            gvBrowserProperties.EmptyDataRowStyle.BorderStyle = BorderStyle.None;
            TemplateBuilder tmpEmptyDataTemplate = new TemplateBuilder();
            tmpEmptyDataTemplate.AppendLiteralString("<strong>No children found.</strong>");
            gvBrowserProperties.EmptyDataTemplate = tmpEmptyDataTemplate;

			return gvBrowserProperties;
		}

		protected string SQLCommandFiltered()
		{
			string strSQL = string.Empty;
			string strSQLColumns = string.Empty;

            foreach (string fieldDesc in viewFields)
            {
                strSQLColumns += FALMTabularFolderBrowserUtility.GetPropertyName(fieldDesc) + ",";
            }
			
			if (strSQLColumns.Length > 1)
			{
                strSQL = "SELECT ID AS EditID," + strSQLColumns.Substring(0, strSQLColumns.Length - 1) + " FROM " + ViewName + " ";
            }
			else
			{
                strSQL = "SELECT ID AS EditID, * FROM " + ViewName + " ";
			}

            if (showAllDescendants)
			{
				strSQL += "WHERE (path LIKE '%," + ((umbraco.cms.businesslogic.datatype.DefaultData) (_data)).NodeId + ",%') ";
			}
			else
			{
				strSQL += "WHERE (parentId = " + ((umbraco.cms.businesslogic.datatype.DefaultData) (_data)).NodeId + ") ";
			}


            string strAlias = string.Empty;
            string strDbType = string.Empty;
            string strText = string.Empty;

            TextBox txtBox;
            DateTimePicker dtpckDateFieldFrom;
            DateTimePicker dtpckDateFieldTo;
            bool validFrom;
            bool validTo;
            DateTime dFrom = DateTime.Now;
            DateTime dTo = DateTime.Now;
            int i = 0;

            // Table Filter Cells
            foreach (string fieldDesc in searchFields)
            {
                // Retrieve DbType of the filter property
                strText = FALMTabularFolderBrowserUtility.GetPropertyText(fieldDesc);
                strAlias = FALMTabularFolderBrowserUtility.GetPropertyName(fieldDesc);
                strDbType = (string)fieldSet[strAlias];

                switch (strDbType)
                {
                    case "Date":
                        dtpckDateFieldFrom = (DateTimePicker)pnlFilterProperties.FindControl("dtpck" + strAlias + "From");
                        dtpckDateFieldTo = (DateTimePicker)pnlFilterProperties.FindControl("dtpck" + strAlias + "To");
                        if (dtpckDateFieldFrom != null)
                        {
                            validFrom = DateTime.TryParse(dtpckDateFieldFrom.Text, out dFrom);
                        }
                        else
                        {
                            validFrom = false;
                        }
                        if (dtpckDateFieldTo != null)
                        {
                            validTo = DateTime.TryParse(dtpckDateFieldTo.Text, out dTo);
                        }
                        else
                        {
                            validTo = false;
                        }

                        if (validFrom && validTo)
                        {
                            strSQL += string.Format("AND ({0} >= CONVERT(DATETIME, '{1} 00:00:00', 102) AND {0} < CONVERT(DATETIME, '{2} 23:59:59', 102)) ", strAlias, dFrom.ToString("yyyy.MM.dd"), dTo.ToString("yyyy.MM.dd"));
                        }
                        else if (validFrom)
                        {
                            strSQL += string.Format("AND ({0} >= CONVERT(DATETIME, '{1} 00:00:00', 102)) ", strAlias, dFrom.ToString("yyyy.MM.dd"));
                        }
                        else if (validTo)
                        {
                            strSQL += string.Format("AND ({0} < CONVERT(DATETIME, '{1} 23:59:59', 102)) ", strAlias, dTo.ToString("yyyy.MM.dd"));
                        }
                        break;

                    case "Int":
                        txtBox = (TextBox)pnlFilterProperties.FindControl("txtb" + strAlias);
                        if (txtBox != null && txtBox.Text != "")
                        {
                            if (int.TryParse(txtBox.Text, out i))
                            {
                                strSQL += string.Format("AND ({0} = {1}) ", strAlias, i);
                            }
                        }
                        break;

                    case "Nvarchar":
                        txtBox = (TextBox)pnlFilterProperties.FindControl("txtb" + strAlias);
                        if (txtBox != null && txtBox.Text != "")
                        {
                            strSQL += string.Format("AND ({0} LIKE '%{1}%') ", strAlias, txtBox.Text);
                        }
                        break;

                    case "Ntext":
                        break;

                    default:
                        break;
                }
            }

			return strSQL;
		}

		protected void btnSearch_Click(object sender, EventArgs e)
		{
            sqlDSBrowserProperties.SelectCommand = SQLCommandFiltered();
            gvBrowserProperties.DataBind();
        }
	}
}
