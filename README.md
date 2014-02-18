FALM-Tabular-Folder-Viewer-v0.9x4.5
===================================
Package for Umbraco 4.5+
This package gives content editors the ability to browse site contents in a searchable, developers defined, record like, table.

This is mostly useful when dealing with a great number of content nodes (e.g. news): in such a case the Umbraco best practices suggest to organize content in folders (e.g. by using Autofolder), in order to keep the content tree manageable for the editors, but this can make it hard for the editors to find a certain node buried within the content tree.
Enters Tabular Folder Browser: with it you can add a tab to your folder that let your editors search and display its children in a record-like, paged, table. The properties to be searched for and to be displayed in the table’s columns are defined by the developers, can be specific to each folder type, and include both standard and user defined properties.
From the table view the editors can enter the standard editing page for content nodes.

All you need to do is create a new datatype and use the pre value editor to define the properties to be searched for and the properties to be displayed in columns: use the property alias for user defined properties and the following names for standard Umbraco properties: ID, name, createBy, createDate, documentType, template, lastPublished, updateDate, releaseDate, expireDate.
When defining the properties to be displayed in columns, you should specify the width of each column by adding an HTML width name, enclosed in [ ] (E.G. name[30%], title[30%], createBy[20%], createDate[10%], documentType[10%]).
After creating the datatype, just adds it to a dedicated tab in your folder doc type, and you are done.

In order to work, TFB adds an SQL view to Umbraco DB for each datatype definition. This view cleverly maps standard and user defined properties to SQL columns, thus enabling an efficient and easy way to display content documents as if they where records of a DB.
This means that you need the appropriate DB rights when defining or modifying a datatype, but no special rights to use it.

We have developed and tested it under SQL2005, so I’m not sure it will work with MySQL. If somebody could test it and post the results, it would be great.

I have made a quick port to 4.5, you may need to reload the prevalue editor page after creating a new Datatype, in order to complete the creation process.

As usual, remember to vote for this package if you find it useful.
