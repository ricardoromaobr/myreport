// 
// Report.cs
//  
// Author:
//       Tomasz Kubacki <Tomasz.Kubacki(at)gmail.com>
// 
// Copyright (c) 2010 Tomasz Kubacki 2010
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using MyReport.Model.Controls;
using MyReport.Model.Data;

namespace MyReport.Model;

public class Report
{

	public Report ()
	{
		Width = 538.582677165354; // 210 - 20  = 190 mm (-20mm for marings)
		Height = 785.196850393701;//297 - 20 = 277 mm (-20mm for marings)
		Margin = new Thickness(28.3464566929134);
		Groups = new List<Group> ();			
		Parameters = new List<Field> ();
		DataFields = new List<Field> ();
		GroupHeaderSections = new List<GroupHeaderSection> ();
		GroupFooterSections = new List<GroupFooterSection> ();
		Pages = new List<Page> ();
		ResourceRepository = new Dictionary<string,byte[]> ();
		ReportHeaderSection = new ReportHeaderSection { Location = new Point (0, 0), Size = new Size (Width, 50) };
		PageHeaderSection = new PageHeaderSection { Location = new Point (0, 0), Size = new Size (Width, 30) };
		DetailSection = new DetailSection { Location = new Point (0, 150), Size = new Size (Width, 50) };
		PageFooterSection = new PageFooterSection { Location = new Point (0, 300), Size = new Size (Width, 30) };
		ReportFooterSection = new ReportFooterSection { Location = new Point (0, 300), Size = new Size (Width, 30) };
	}

	public string Title { get; set; }
		
	public string DataScript {get;set;}

	public PageHeaderSection PageHeaderSection { get; set; }

	public PageFooterSection PageFooterSection { get; set; }
		
	public ReportHeaderSection ReportHeaderSection { get; set; }

	public ReportFooterSection ReportFooterSection { get; set; }

	public DetailSection DetailSection { get; internal set; }

	public List<GroupHeaderSection> GroupHeaderSections { get; set; }

	public List<GroupFooterSection> GroupFooterSections { get; set; }

	public List<Page> Pages { get; internal set; }
		
	public List<Field> Parameters { get; private set; }
		
	public List<Field> DataFields { get; private set; }
		
	public List<Field> Expression { get; private set; }

	public List<Group> Groups { get; internal set; }

	public Dictionary<string,byte[]> ResourceRepository { get; set; }

	public double Height { get; set; }
		
	public double HeightWithMargins { get { return Height  + Margin.Top + Margin.Bottom;} }		
		
	double width;
	public double Width { 
		get { return width; } 
		set {
			if (width != value)
			{
				width = value;
				if (ReportHeaderSection != null)
				{
					ReportHeaderSection.Width = width;
					ReportFooterSection.Width = width;
					PageHeaderSection.Width = width;
					PageFooterSection.Width = width;
					DetailSection.Width = width;

					foreach (var item in GroupHeaderSections)
					{
						item.Width = width;

					}

					foreach (var item in GroupFooterSections)
					{
						item.Width = width;
					}
				}
			}
		} 
        
	}
		
	public double WidthWithMargins { get { return Width  + Margin.Left + Margin.Right;} }
		
	public Thickness Margin {get;set;}

	public UnitType Unit { get; set; }

	public void AddGroup (string fieldName)
	{
		Group gr = new Group { GroupingFieldName = fieldName };
		Groups.Add (gr);
		GroupHeaderSection gh = new GroupHeaderSection { Name = "Group header " + gr.GroupingFieldName, Size = new Size (Width, 20), Location = new Point (0, 150) };
		GroupHeaderSections.Add (gh);
		GroupFooterSection gf = new GroupFooterSection { Name = "Group footer " + gr.GroupingFieldName, Size = new Size (Width, 20), Location = new Point (0, 250) };
		GroupFooterSections.Add (gf);
	}

	public void RemoveGroup (Group gr)
	{
		int index = Groups.IndexOf (gr);
		if (index != -1) {
			Groups.RemoveAt (index);
			GroupHeaderSections.RemoveAt (index);
			GroupFooterSections.RemoveAt (index);
		}
	}

	object dataSource;

	public object DataSource { 
		get { return dataSource; } 
		set {
			dataSource = value;
			DataFields = new List<Field> ();	
			if (dataSource != null ) {

				Type dsType = dataSource.GetType ();
				Type elementType = dsType.GetElementType ();
				if (elementType == null) {
					Type genericArgumentTypes = dsType.GetGenericArguments () [0];
					Type genericType = typeof(ObjectDataSource<>); 
					var genericDatasourceType = genericType.MakeGenericType (new Type[]{genericArgumentTypes});
					_dataSource = (Activator.CreateInstance (genericDatasourceType, dataSource))  as IDataSource;
				} else {
					Type genericType = typeof(ObjectDataSource<>); 
					var realGeneric = genericType.MakeGenericType (new Type[]{elementType});
					_dataSource = (Activator.CreateInstance (realGeneric, dataSource))  as IDataSource;
				}
				FillFieldsFromDataSource ();
			}else {
				_dataSource = null;
			}
				
		} 
		
	}

	internal IDataSource _dataSource {get; set;}
 
	public void FillFieldsFromDataSource ()
	{
		DataFields = new List<Field> ();				
		if (DataSource != null) {
				
			foreach(var field in _dataSource.DiscoverFields () ){
				
				field.FieldKind = FieldKind.Data;
				DataFields.Add( field );				 					
			}
		}else 
			throw new InvalidOperationException("Datasource can't be null while discovering data fields");
				
	}
		
	public void CopyToReport (Report r) {
		r.Title = Title;
		r.ResourceRepository = ResourceRepository;
		r.PageHeaderSection = PageHeaderSection;
		r.PageFooterSection = PageFooterSection;
		r.ReportHeaderSection = ReportHeaderSection;
		r.ReportFooterSection = ReportFooterSection;
		r.DataScript = DataScript;
		r.DetailSection = DetailSection;
		r.GroupHeaderSections = GroupHeaderSections;
		r.GroupFooterSections = GroupFooterSections;
		r.Height = Height;
		r.Width = Width;
		r.Margin = Margin;
		r.Unit = Unit;			
	}
}