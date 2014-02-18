using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web.SessionState;

using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

using umbraco.editorControls;

namespace FALMTabularFolderBrowser
{
	class FALMTabularFolderBrowserPrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
	{
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

        // referenced datatype
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        // Data Type properties
		private Label _lblDBType;
        private Label _lblViewName;
        private CheckBox _chkbShowAllDescendants;
		private TextBox _txtbSearchProperties;
		private TextBox _txtbDisplayProperties;
		private TextBox _txtbSQLWhereClause;

        private LinkButton _lbtSearchHelp;
        private LinkButton _lbtDisplayHelp;
        private LinkButton _lbtWhereHelp;

        private string _errorMsg = string.Empty;
        private string _helpMsg = string.Empty;
        private string _viewCreateCode = string.Empty;
        private string _fieldsList = string.Empty;

        private string _standardProperties = ",ID:Integer,name:Nvarchar,createBy:Nvarchar,createDate:Date,documentType:Nvarchar,template:Nvarchar,lastPublished:Date,updateDate:Date,releaseDate:Date,expireDate:Date,";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DataType"></param>
		public FALMTabularFolderBrowserPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType)
        {
            // state it knows its datatypedefinitionid
            _datatype = DataType;
            setupChildControls();

        }

		/// <summary>
		/// Setup child controls
		/// </summary>
		private void setupChildControls()
        {
			_lblDBType = new Label();
			_lblDBType.ID = "dbtype";

            _lblViewName = new Label();
            _lblViewName.ID = "viewName";
            
            _chkbShowAllDescendants = new CheckBox();
			_chkbShowAllDescendants.ID = "chkbShowAllDescendants";

			_txtbSearchProperties = new TextBox();
            _txtbSearchProperties.ID = "txtbSearchProperties";
			_txtbSearchProperties.CssClass = "umbEditorTextField";
			_txtbSearchProperties.TextMode = TextBoxMode.SingleLine;

			_txtbDisplayProperties = new TextBox();
            _txtbDisplayProperties.ID = "txtbDisplayProperties";
			_txtbDisplayProperties.CssClass = "umbEditorTextField";
			_txtbDisplayProperties.TextMode = TextBoxMode.SingleLine;

			_txtbSQLWhereClause = new TextBox();
            _txtbSQLWhereClause.ID = "txtbSQLWhereClause";
			_txtbSQLWhereClause.CssClass = "umbEditorTextField";
			_txtbSQLWhereClause.TextMode = TextBoxMode.MultiLine;

            _lbtSearchHelp = new LinkButton();
            _lbtSearchHelp.ID = "SearchHelp";
            _lbtSearchHelp.Text = "&nbsp;?&nbsp;";
            _lbtSearchHelp.CommandName = "Search";
            _lbtSearchHelp.Command += new CommandEventHandler(_lbtHelp_Command);

            _lbtDisplayHelp = new LinkButton();
            _lbtDisplayHelp.ID = "DisplayHelp";
            _lbtDisplayHelp.Text = "&nbsp;?&nbsp;";
            _lbtDisplayHelp.CommandName = "Display";
            _lbtDisplayHelp.Command += new CommandEventHandler(_lbtHelp_Command);

            _lbtWhereHelp = new LinkButton();
            _lbtWhereHelp.ID = "WhereHelp";
            _lbtWhereHelp.Text = "&nbsp;?&nbsp;";
            _lbtWhereHelp.CommandName = "Where";
            _lbtWhereHelp.Command += new CommandEventHandler(_lbtHelp_Command);

            // put the childcontrols in context - ensuring that
            // the viewstate is persisted etc.
            Controls.Add(_lblViewName);
            Controls.Add(_chkbShowAllDescendants);
			Controls.Add(_txtbSearchProperties);
			Controls.Add(_txtbDisplayProperties);
			Controls.Add(_txtbSQLWhereClause);

            Controls.Add(_lbtSearchHelp);
            Controls.Add(_lbtDisplayHelp);
            Controls.Add(_lbtWhereHelp);
        }

        void _lbtHelp_Command(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Search":
                    _helpMsg += @"<p><strong>Properties to search for</strong></p>
                                  <p>Here you can enter a comma separated list of properties that your editors will be able to search for in the generated control.</p>
                                  <p>You can specify both user defined properties and standard Umbraco properties:</p>
                                  <ul>
                                      <li>for user defined properties just use the property alias as defined in a Document Type.</li>
                                      <li>for standard Umbraco properties you can choose among this properties:<br/>
                                          <i>ID, name, createBy, createDate, documentType, template, lastPublished, updateDate, releaseDate, expireDate</i><br/>
                                          which map to those in the ""Generic properties"" tab.</li>
                                  </ul>";
                    break;

                case "Display":
                    _helpMsg += @"<p><strong>Properties to display</strong></p>
                                  <p>Here you can enter a comma separated list of properties that will map to table columns in the generated control.</p>
                                  <p>You can specify both user defined properties and standard Umbraco properties:</p>
                                  <ul>
                                      <li>for user defined properties just use the property alias as defined in a Document Type.</li>
                                      <li>for standard Umbraco properties you can choose among this properties:<br/>
                                          <i>ID, name, createBy, createDate, documentType, template, lastPublished, updateDate, releaseDate, expireDate</i><br/>
                                          which map to those in the ""Generic properties"" tab.</li>
                                  </ul>
                                  <p>You can specify the witdh of each column by adding an HTML width to the property name, enclosed in [].<br/>
                                     E.G.:<br/>
                                     name[30%], title[30%], createBy[20%], createDate[10%], documentType[10%]</p>";
                    break;

                case "Where":
                    _helpMsg += @"<p><strong>SQL WHERE clause</strong></p>
                                  <p>Here you can enter an extra condition to filter the documents that will be presented to your editors.</p>
                                  <p>The condition is expressed as a standard SQL WHERE clause that may use the user defined properties specified in the ""Search for"" and ""Display"" texboxes and any standard Umbraco property.<br/>
                                     The SQL WHERE clause will be added to the created view.<br/>
                                     E.G.:</p>
                                  <p syle=""padding-left: 2em; font-family: monospace;"">
                                     createBy<>'Administrator'<br/>
                                     AND<br/>
                                     documentType='Page'<br/>
                                  </p>
                                ";
                    break;

                default:
                    break;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                // Split configuration properties into an array string
				string[] config = Configuration.Split("|".ToCharArray());
                
				// Check if configuration properties already have been set
				if (config.Length > 1)
                {
                    _chkbShowAllDescendants.Checked = Convert.ToBoolean(config[1]);
                    _txtbSearchProperties.Text = config[2];
                    _txtbDisplayProperties.Text = config[3];
					_txtbSQLWhereClause.Text = config[4];
                    _lblViewName.Text = config[5];
                }
                else
                {
                    _chkbShowAllDescendants.Checked = false;
					_txtbSearchProperties.Text = "";
					_txtbDisplayProperties.Text = "";
					_txtbSQLWhereClause.Text = "";
                    _lblViewName.Text = "";
                }
                if (_lblViewName.Text == "")
                    _lblViewName.Text = "v" + NormalizeViewName(DataTypeDefinition.GetDataTypeDefinition(_datatype.DataTypeDefinitionId).Text);
                _lblDBType.Text = umbraco.editorControls.DBTypes.Nvarchar.ToString();
            }
        }

		/// <summary>
		/// 
		/// </summary>
		public Control Editor
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// 
		/// </summary>
        public void Save()
        {
            // Create view associated with this instance
            EnsureBaseViews();
            BuildViewCreateCode(_lblViewName.Text);
            CreateView(_lblViewName.Text, _viewCreateCode, true);

            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), umbraco.editorControls.DBTypes.Nvarchar.ToString(), true);
			
			// Generate datatype value as string
            string data = umbraco.editorControls.DBTypes.Nvarchar.ToString() + "|" + _chkbShowAllDescendants.Checked.ToString() + "|" + _txtbSearchProperties.Text + "|" + _txtbDisplayProperties.Text + "|" + _txtbSQLWhereClause.Text + "|" + _lblViewName.Text + "|" + _fieldsList;

			// If the add new prevalue textbox is filled out - add the value to the collection.
			IParameter[] SqlParams = new IParameter[] {
			            SqlHelper.CreateParameter("@value",data),
						SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
			SqlHelper.ExecuteNonQuery("DELETE FROM cmsDataTypePreValues WHERE datatypenodeid = @dtdefid", SqlParams);
			SqlHelper.ExecuteNonQuery("INSERT INTO cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) VALUES (@dtdefid,@value,0,'')", SqlParams);

            // Refresh labels
            _lblDBType.Text = umbraco.editorControls.DBTypes.Nvarchar.ToString();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            BuildViewCreateCode(_lblViewName.Text);

            if (_helpMsg != "")
            {
                writer.Write("<div class=\"propertyItem\"><div class=\"success\" style=\"display: block;\">");
                writer.Write(_helpMsg);
                writer.Write("</div><br style=\"clear: both\" /></div>");
            }
            if (_errorMsg != "")
            {
                writer.Write("<div class=\"propertyItem\"><div class=\"error\" style=\"display: block;\">");
                writer.Write("<p><strong>Error(s) occurred</strong></p><p>");
                writer.Write(_errorMsg);
                writer.Write("</p></div><br style=\"clear: both\" /></div>");
            }
            writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">Database datatype:</div>");
			_lblDBType.RenderControl(writer);
			writer.Write("<br style=\"clear: both\" /></div>");

            writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">View name:</div>");
            _lblViewName.RenderControl(writer);
            writer.Write("<br style=\"clear: both\" /></div>");
			
			writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">Show all descendants:</div>");
			_chkbShowAllDescendants.RenderControl(writer);
			writer.Write("<br style=\"clear: both\" /></div>");

			writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">Properties to search for:<br /><span style=\"color: grey\">(comma separated list of property aliases)</span></div>");
            _txtbSearchProperties.RenderControl(writer);
            writer.Write("&nbsp;&nbsp;");
            _lbtSearchHelp.RenderControl(writer);
			writer.Write("<br style=\"clear: both\" /></div>");

			writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">Properties to display:<br /><span style=\"color: grey\">(comma separated list of property aliases)</span></div>");
            _txtbDisplayProperties.RenderControl(writer);
            writer.Write("&nbsp;&nbsp;");
            _lbtDisplayHelp.RenderControl(writer);
            writer.Write("<br style=\"clear: both\" /></div>");

			writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">SQL WHERE clause:</div>");
            _txtbSQLWhereClause.RenderControl(writer);
            writer.Write("&nbsp;&nbsp;");
            _lbtWhereHelp.RenderControl(writer);
            writer.Write("<br style=\"clear: both\" /></div>");

            writer.Write("<div class=\"propertyItem\"><div class=\"propertyItemheader\">View creation code:</div><div class=\"propertyItemContent\"><pre style=\"margin: 0; border: #ccc 1px solid; padding: 3px; color: grey;\">");
            writer.Write(_viewCreateCode);
            writer.Write("</pre></div><br style=\"clear: both\" /></div>");
        }

		/// <summary>
		/// 
		/// </summary>
        public string Configuration
        {
            get
            {
                object conf = SqlHelper.ExecuteScalar<object>("SELECT value FROM cmsDataTypePreValues WHERE datatypenodeid = @datatypenodeid", SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                if (conf != null)
                    return conf.ToString();
                else
                    return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName"></param>
        private string NormalizeViewName(string viewName)
        {
            return viewName.Replace(" ", "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="sqlViewCreationCode"></param>
        /// <param name="dropIfExists"></param>
        private void CreateView(string viewName, string sqlViewCreationCode, bool dropIfExists)
        {
            string queryForView = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = '{0}'", viewName);
            bool viewExists;
            
            if (SqlHelper.ExecuteScalar<string>(queryForView) == viewName)
            {
                viewExists = true;
            }
            else
            {
                viewExists = false;
            }

            if (viewExists && dropIfExists)
            {
                try
                {
                    SqlHelper.ExecuteNonQuery("DROP VIEW " + viewName);
                    viewExists = false;
                }
                catch (Exception ex)
                {
                    _errorMsg += string.Format("<strong>Error deleting view {0}:</strong><br />Exception message: {1}<br />Inner exception message: {2}", viewName, ex.Message, ex.InnerException.Message);
                }
            }

            if (!viewExists)
            {
                try
                {
                    SqlHelper.ExecuteNonQuery(sqlViewCreationCode);
                }
                catch (Exception ex)
                {
                    _errorMsg += string.Format("<strong>Error creating view {0}:</strong><br />Exception message: {1}<br />Inner exception message: {2}", viewName, ex.Message, ex.InnerException.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strViewName"></param>
        private void EnsureBaseViews()
        {
            string createFullPropertyData = @"
CREATE VIEW vFALMFullPropertyData AS
  SELECT
    cmsPropertyData.contentNodeId,
    cmsPropertyData.versionId,
    cmsPropertyData.dataInt,
    cmsPropertyData.dataDate,
    cmsPropertyData.dataNvarchar,
    cmsPropertyData.dataNtext,
    cmsPropertyType.contentTypeId,
    cmsPropertyType.Alias,
    cmsPropertyType.Name,
    cmsPropertyType.tabId,
    cmsPropertyType.sortOrder,
    cmsDataType.dbType
  FROM
    cmsPropertyData
      INNER JOIN
    cmsPropertyType ON cmsPropertyData.propertyTypeId = cmsPropertyType.id
      INNER JOIN
    cmsDataType ON cmsDataType.nodeId = cmsPropertyType.dataTypeId
";
            CreateView("vFALMFullPropertyData", createFullPropertyData, false);

            string createFullContent = @"
CREATE VIEW vFALMFullContent AS
  SELECT
    doc.text AS name,
    nodeUser.userName AS createBy,
    node.createdate AS createDate,
    node.id ID,
    ctNode.text AS documentType,
    dctNode.text AS template,
    CASE WHEN publ.published IS NULL THEN NULL ELSE ver.VersionDate END AS lastPublished,
    doc.updateDate,
    doc.releaseDate,
    doc.expireDate,
    node.parentID,
    node.path,
	ver.VersionId,
    node.[level]
  FROM
    umbracoNode node
      INNER JOIN 
    umbracoUser nodeUser ON nodeUser.id = node.nodeUser
      INNER JOIN 
    cmsContentVersion ver ON ver.id = (SELECT MAX(V.id) FROM cmsContentVersion V WHERE V.ContentId = node.Id)
      INNER JOIN 
    cmsDocument doc ON doc.versionId = ver.VersionId
      INNER JOIN 
    cmsContent cont ON cont.Nodeid = doc.nodeId
      INNER JOIN
    cmsContentType ct ON ct.nodeId = cont.contentType
      INNER JOIN 
    umbracoNode ctNode ON ctNode.id = ct.nodeId
      LEFT OUTER JOIN
    cmsDocumentType docType ON docType.contentTypeNodeId = cont.contentType AND docType.IsDefault = 1 
      INNER JOIN 
    umbracoNode dctNode ON dctNode.id = coalesce(templateId, docType.templateNodeId)
      LEFT OUTER JOIN
    cmsDocument publ ON publ.nodeId = node.id AND publ.published = 1
  WHERE
    node.nodeObjectType = 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972' and node.parentID <> -20
";
            CreateView("vFALMFullContent", createFullContent, false);

        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="strViewName"></param>
		private void BuildViewCreateCode(string strViewName)
		{
			string[] arrAllProperties = MergeArrayWithoutDuplicates();

			// ****** SQL for Property Set ******
			string strSQLColumns = string.Empty;
			string strSQLJoins = string.Empty;
			string strSQLCondition = string.Empty;
            string strSQLWhereClause = string.Empty;
            string strAlias;
			string strDbType;

            // We can't use alias name in WHERE clause, so we prepare for some replacements
            if (!_txtbSQLWhereClause.Text.Equals(string.Empty))
                strSQLWhereClause = _txtbSQLWhereClause.Text;

            if (arrAllProperties.Length == 0)
            {
                _errorMsg += "No property to display or to search for<br />";
            }
            else
            {
                for (int i = 0; i < arrAllProperties.Length; i++)
                {
                    strAlias = arrAllProperties[i];

                    int stdPropertyIdx = _standardProperties.IndexOf("," + strAlias + ":");
                    if (stdPropertyIdx < 0)
                    {
                        // strAlias is a user defined property, so we must add it to the view

                        // Retrieve DBType for property strAlias
                        strDbType = SqlHelper.ExecuteScalar<string>("SELECT DISTINCT cmsDataType.dbType FROM cmsPropertyType INNER JOIN cmsDataType ON cmsDataType.nodeId = cmsPropertyType.dataTypeId WHERE cmsPropertyType.Alias  = '" + strAlias + "'");

                        if (strDbType == "Integer")
                            strDbType = "Int";

                        if (strDbType != null)
                        {
                            // Declare the column as an SQL alias of the right propertyData column
                            strSQLColumns += string.Format(",\n  P{0}.data{1} AS '{2}'", i, strDbType, strAlias);
                            // Left join with the propertyData table, so that we get <null> if there is no property for a document
                            strSQLJoins += string.Format("    LEFT OUTER JOIN\n  vFALMFullPropertyData P{0} ON vFALMFullContent.versionID = P{0}.versionId AND P{0}.Alias = '{1}'\n", i, strAlias);
                            // Replace any eventual occurrence of this column in the WHERE clause with the unaliased definition
                            strSQLWhereClause = strSQLWhereClause.Replace(strAlias, string.Format("P{0}.data{1}", i, strDbType));

                            _fieldsList += "," + strAlias + ":" + strDbType;
                        }
                        else
                        {
                            _errorMsg += string.Format("Unknown property: {0}<br />", strAlias);
                        }
                    }
                    else
                    {
                        int stdTypeStart = _standardProperties.IndexOf(":", stdPropertyIdx);
                        int stdTypeEnd = _standardProperties.IndexOf(",", stdTypeStart);

                        strDbType = _standardProperties.Substring(stdTypeStart+1, stdTypeEnd-stdTypeStart-1);
                        _fieldsList += "," + strAlias + ":" + strDbType;
                    }
                }
                if (_fieldsList.Length > 0)
                    _fieldsList = _fieldsList.Substring(1);
            }

			// add the WHERE clause
			if (strSQLWhereClause != "")
				strSQLCondition = "WHERE\n  " + strSQLWhereClause;

            _viewCreateCode = string.Format("CREATE VIEW {0} AS\nSELECT\n  vFALMFullContent.*{1}\nFROM\n  vFALMFullContent\n{2}{3}\n", strViewName, strSQLColumns, strSQLJoins, strSQLCondition);

            return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private string[] MergeArrayWithoutDuplicates()
        {
			// Split filter properties into an array string
            string[] arrFilterProperties = _txtbSearchProperties.Text.Replace(" ", "").Split(',');
			
			// Split properties into an array string
            string[] arrProperties = _txtbDisplayProperties.Text.Replace(" ", "").Split(',');

			ArrayList arrAllProperties = new ArrayList();
            string p;

			foreach (string s in arrFilterProperties)
			{
                p = FALMTabularFolderBrowserUtility.GetPropertyName(s);
				if (p!="" && !arrAllProperties.Contains(p))
				{
					arrAllProperties.Add(p);
				}
			}

			foreach (string s in arrProperties)
			{
                p = FALMTabularFolderBrowserUtility.GetPropertyName(s);
                if (s != "" && !arrAllProperties.Contains(p))
				{
					arrAllProperties.Add(p);
				}
			}

			return (string[])arrAllProperties.ToArray(typeof(string));
        }

	}
}
