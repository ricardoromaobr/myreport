// ReportEngine.cs
//  
// Author:
//       Tomasz Kubacki <Tomasz.Kubacki (at) gmail.com>
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

namespace MyReport.Model.Engine;

public class ReportEngine
{
	internal IReportRenderer ReportRenderer;
	Report Report;
	IDataSource source;
	internal ReportContext context;
	internal Page currentPage = null;
	bool beforeFirstDetailSection = true;
	Section currentSection = null;
	List<SpanInfo> currentSectionSpans = null;
	List<Control> currentSectionOrderedControls = null;
	List<Control> currentSectionControlsBuffer = null;
	List<Control> currentPageFooterSectionControlsBuffer = null;
	List<Control> subreportSectionControlsBuffer = null;
	bool afterReportHeader = false;

	public List<Control> CurrentPageFooterSectionControlsBuffer {
		get => this.currentPageFooterSectionControlsBuffer;
		set => currentPageFooterSectionControlsBuffer = value;
	}
	
	List<Line> currentSectionExtendedLines = null;
	double spanCorrection = 0;
	public bool IsSubreport { get; set; }

	bool dataSourceHasNextRow = true;
	bool stop = false;
	Dictionary<string, Field> parameters;

	public ReportEngine (Report report, IReportRenderer renderer)
	{
		Report = report;
		source = Report._dataSource;
		parameters = Report.Parameters.ToDictionary (rp => rp.Name);
		if (source == null)
			source = new DummyDataSource ();
		ReportRenderer = renderer;
		currentSectionSpans = new List<SpanInfo> ();
		currentSectionOrderedControls = new List<Control> ();
		currentSectionExtendedLines = new List<Line> ();
		currentSectionControlsBuffer = new List<Control> ();
		subreportSectionControlsBuffer = new List<Control> ();
		currentPageFooterSectionControlsBuffer = new List<Control> ();
		
		context = new ReportContext
		{
			CurrentPageIndex = 0, 
			DataSource = null, 
			Parameters = new Dictionary<string, string> (), 
			ReportMode = ReportMode.Preview
		};
		
		Report.Pages = new List<Page> ();
		
		NextPage ();
		SelectCurrentSectionByTemplateSection (Report.PageFooterSection);
	}

	public void Process ()
	{
		nextRecord ();

		while (!ProcessReportPage ()) {
			NextPage ();
		}
		
		for (int i = 0; i < Report.Pages.Count; i++) {
			foreach (var item in Report.Pages [i].Controls) {
				if (item is IDataControl) {
					IDataControl dc = item as IDataControl;
					if (dc.FieldName == "#NumberOfPages") {
						dc.Text = Report.Pages.Count.ToString ();
					}
				}
			}
		}
		
		if (source != null)
			source.Reset ();

		OnAfterReportProcess ();
	}

	void nextRecord ()
	{
		dataSourceHasNextRow = source.MoveNext ();
		context.RowIndex++;
	}

	public bool ProcessReportPage ()
	{
		bool result = false;
		stop = false;

		do {
			currentSection.TemplateControl.FireBeforeControlProcessing (context, currentSection);

			if (!currentSectionControlsBuffer.Contains (currentSection))
				currentSectionControlsBuffer.Add (currentSection);

			result = ProcessSectionUpToHeightTreshold (context.HeightLeftOnCurrentPage);

			if (!result && currentSection.KeepTogether)
				currentSectionControlsBuffer.Clear ();

			AddControlsToCurrentPage (context.HeightUsedOnCurrentPage);

			context.HeightLeftOnCurrentPage -= currentSection.Height;
			context.HeightUsedOnCurrentPage += currentSection.Height;

			if (result) {
				nextSection ();
			} else {
				return false;
			}
		} while (!stop);

		return result;
	}
	
	Dictionary<string, List<Control>> controlsFromPreviousSectionPage = new Dictionary<string, List<Control>> ();

	T SelectCurrentSectionByTemplateSection<T> (T s) where T : Section
	{
		T newSection = null;
		if (controlsFromPreviousSectionPage.ContainsKey (s.Name)) {
			currentSectionOrderedControls = controlsFromPreviousSectionPage [s.Name];
			controlsFromPreviousSectionPage.Remove (s.Name);
			newSection = currentSectionOrderedControls [0] as T;
			currentSectionOrderedControls.RemoveAt (0);
		} else {
			newSection = s.CreateControl () as T;
			newSection.Format ();
			currentSectionOrderedControls = newSection.Controls.OrderBy (ctrl => ctrl.Top).ToList ();
		}
		currentSectionSpans.Clear ();
		currentSectionExtendedLines.Clear ();
		newSection.Location = new Point (s.Location.X, 0);
		currentSection = newSection;

		currentSectionControlsBuffer.Clear ();
		return newSection;
	}

	double marginBottom = 0;

	/// <summary>
	/// Processes the section up to heightTreshold.
	/// </summary>
	/// <returns>
	///  returns <c>true</c> if finished processig section and <c>false</c> while not
	/// </returns>
	/// <param name='pageBreakTreshold'>
	/// maxiumum height (starting from current section Location.Y) after which page will break
	/// </param>
	public bool ProcessSectionUpToHeightTreshold (double heightTreshold)
	{
		double span = 0;
		double y = 0;
		double maxHeight = 0;
		double maxControlBottom = 0;
		double tmpSpan = 0;
		bool result = true;
		double realBreak = 0;
		double breakControlMax = 0;
		bool allKeepTogether = false;
		double heightTresholdIncludingBottomMargin = 0;

		if (currentSectionOrderedControls.Count > 0) {
			maxControlBottom = currentSectionOrderedControls.Max (ctrl => ctrl.Bottom);
		}
		
		marginBottom = currentSection.Height - maxControlBottom;
		if (marginBottom < 0) marginBottom = 0;
		
		heightTresholdIncludingBottomMargin = heightTreshold - marginBottom;

		for (int i = 0; i < currentSectionOrderedControls.Count; i++) {
			var control = currentSectionOrderedControls [i];
			tmpSpan = double.MinValue;

			if (!control.IsVisible)
				continue;

			if (control is Line && (control as Line).ExtendToBottom) {
				var line = control as Line;
				currentSectionExtendedLines.Add (line);
			}

			if (source != null && control is IDataControl) {
				IDataControl dc = control as IDataControl;
				if (!string.IsNullOrEmpty (dc.FieldName)) {

					switch (dc.FieldKind) {
						case FieldKind.Parameter:
							if (parameters.ContainsKey (dc.FieldName)) {
								var parameter = parameters [dc.FieldName];
								dc.Text = parameter.GetValue (parameter.DefaultValue, dc.FieldTextFormat);
							}
							break;
						case FieldKind.Expression:
							if (dc.FieldName == "#PageNumber") {
								dc.Text = context.CurrentPageIndex.ToString ();
							} else if (dc.FieldName == "#RowNumber") {
								dc.Text = context.RowIndex.ToString ();
							}
							break;
						case FieldKind.Data:
							if (source.ContainsField (dc.FieldName))
								dc.Text = source.GetValue (dc.FieldName, dc.FieldTextFormat);
							break;
						default:
							break;
					}
				}
			}

			y = control.Top + span;
			Size controlSize = ReportRenderer.MeasureControl (control);

			foreach (SpanInfo item in currentSectionSpans) {
				if (y > item.Treshold) {
					tmpSpan = Math.Max (tmpSpan, item.Span);
				}
			}

			span = tmpSpan == double.MinValue ? 0 : tmpSpan;
			control.Top += span;

			if (control is SubReport) {
				SubReport sr = control as SubReport;
				double maxSubreportHeight = ((heightTreshold - span) - sr.Top);
				sr.ProcessUpToPage (this.ReportRenderer, maxSubreportHeight);

				if (!(sr.Engine.context.HeightUsedOnCurrentPage > maxSubreportHeight)) {
					controlSize = new Size (sr.Width, sr.Engine.context.HeightUsedOnCurrentPage);
					subreportSectionControlsBuffer.AddRange (sr.Engine.currentPage.Controls);
					sr.Engine.currentPage.Controls.Clear ();

					if (!sr.Finished && sr.CanGrow) {
						StoreSectionForNextPage ();

						var subreportClone = sr.CreateControl () as SubReport;
						subreportClone.Top -= 0;
						subreportClone.Engine = sr.Engine;
						subreportClone.Height = sr.TemplateControl.Height;

						StoreControlForNextSection (subreportClone);
						sr.Engine.NextPage ();
					}
					if (!sr.Finished)
						result = false;
				} else {
					Console.Write ("error:");
				}
			}

			double heightBeforeGrow = control.Height;
			double bottomBeforeGrow = control.Bottom;
			control.Size = controlSize;

			if (control.Bottom <= heightTreshold) {
				if (!allKeepTogether) {
					currentSectionControlsBuffer.Add (control);
				} else {
					StoreSectionForNextPage ();
					var controlToStore = control;
					controlToStore.Top -= realBreak;
					controlToStore.Height = heightBeforeGrow;
					StoreControlForNextSection (controlToStore);
				}
			} else {
				result = false;
				StoreSectionForNextPage ();
				if (!currentSection.KeepTogether) {

					breakControlMax = control.Height - ((control.Top + control.Height) - heightTreshold);
					if (realBreak == 0)
						realBreak = heightTreshold;

					if (control.Top > heightTreshold) {
						var controlToStore = control.CreateControl ();
						controlToStore.Top -= realBreak;
						controlToStore.Height = heightBeforeGrow;
						StoreControlForNextSection (controlToStore);
						continue;
					}
					
					Control [] brokenControl = ReportRenderer.BreakOffControlAtMostAtHeight (control, breakControlMax);
					realBreak = heightTreshold - (breakControlMax - brokenControl [0].Height);
					StoreControlForNextSection (brokenControl [1]);
					currentSectionControlsBuffer.Add (brokenControl [0]);
				} else {
					var controlToStore = control;
					controlToStore.Top -= realBreak;
					controlToStore.Height = heightBeforeGrow;

					if (!allKeepTogether) {

						for (int w = 0; w < currentSectionControlsBuffer.Count; w++) {
							currentSectionControlsBuffer [w].Height = currentSectionControlsBuffer [w].TemplateControl.Height;
							controlsFromPreviousSectionPage [currentSection.Name].Add (currentSectionControlsBuffer [w]);
						}
						allKeepTogether = true;
					}

					StoreControlForNextSection (controlToStore);

					continue;
				}

			}

			if (currentSection.CanGrow && maxHeight <= control.Bottom) {
				maxHeight = Math.Max (control.Bottom, maxHeight);
			}

			if (!result) {
				if (realBreak > 0) {
					maxHeight = Math.Max (realBreak, maxHeight);
				}
			}

			currentSectionSpans.Add (
				new SpanInfo {
					Treshold = bottomBeforeGrow,
					Span = span + control.Bottom - bottomBeforeGrow
				});
		}

		double sectionHeightWithMargin = maxHeight + marginBottom;
		if (!result) {
			currentSection.Height = heightTreshold;
		} else if ((currentSection.CanGrow && currentSection.Height < sectionHeightWithMargin) ||
		           (currentSection.CanShrink && currentSection.Height > sectionHeightWithMargin)) {
			currentSection.Height = sectionHeightWithMargin;
		} else {
			currentSection.Height = Math.Min (currentSection.Height, heightTreshold);
		}

		foreach (Line lineItem in currentSectionExtendedLines) {
			if (lineItem.Location.Y == lineItem.End.Y) {
				lineItem.Location = new Point (lineItem.Location.X, currentSection.Height - lineItem.LineWidth / 2);
				lineItem.End = new Point (lineItem.End.X, currentSection.Height - lineItem.LineWidth / 2);
			} else if (lineItem.Location.Y > lineItem.End.Y) {
				lineItem.Location = new Point (lineItem.Location.X, currentSection.Height);
			} else {
				lineItem.End = new Point (lineItem.End.X, currentSection.Height);
			}

			if (!result) {
				var newCtrl = lineItem.CreateControl ();

				if (lineItem.Location.Y == lineItem.End.Y)
					lineItem.IsVisible = false;
				newCtrl.Top = 0;
				StoreSectionForNextPage ();
				controlsFromPreviousSectionPage [currentSection.Name].Insert (1, newCtrl);
			}
		}
		
		sectionToStore = null;
		if (!currentSection.CanGrow) {
			controlsFromPreviousSectionPage.Remove (currentSection.Name);
			result = true;
		}
		return result;
	}

	Section sectionToStore = null;

	void StoreSectionForNextPage ()
	{
		if (!controlsFromPreviousSectionPage.ContainsKey (currentSection.Name)) {
			sectionToStore = currentSection.CreateControl () as Section;

			var controlsToNextPage = new List<Control> ();
			controlsToNextPage.Add (sectionToStore);
			controlsFromPreviousSectionPage.Add (currentSection.Name, controlsToNextPage);
			sectionToStore.Height = 0;
		}
	}
	void StoreControlForNextSection (Control controlToStore)
	{
		controlsFromPreviousSectionPage [currentSection.Name].Add (controlToStore);
		sectionToStore.Height = Math.Max (sectionToStore.Height, controlToStore.Bottom + marginBottom);
	}
	void nextSection ()
	{
		switch (currentSection.SectionType) {
			case SectionType.PageHeader:
				if (context.CurrentPageIndex > 1) {
					SelectCurrentSectionByTemplateSection (Report.PageFooterSection);
				} else {
					SetDetailsOrGroup ();
				}
				break;
			case SectionType.PageFooter:
				if (!afterReportHeader) {
					SelectCurrentSectionByTemplateSection (Report.ReportHeaderSection);
				} else {
					SetDetailsOrGroup ();
				}
				break;
			case SectionType.ReportHeader:
				if (Report.ReportHeaderSection.BreakPageAfter) {
					NextPage ();
					stop = true;
				} else {
					if (context.CurrentPageIndex == 1) {
						SelectCurrentSectionByTemplateSection (Report.PageHeaderSection);
					} else {
						SetDetailsOrGroup ();
					}
				}
				afterReportHeader = true;
				break;
			case SectionType.Details:
				SetDetailsOrGroup ();
				break;
			case SectionType.ReportFooter:
				AddControlsToCurrentPage (Report.Height - Report.PageFooterSection.Height, currentPageFooterSectionControlsBuffer);
				stop = true;
				break;
			default:
				break;
		}
		
		if (!currentSection.IsVisible)
			nextSection ();
	}
	void SetDetailsOrGroup ()
	{
		if (!controlsFromPreviousSectionPage.ContainsKey (Report.DetailSection.Name) && !beforeFirstDetailSection) {
			nextRecord ();
		}
		if (dataSourceHasNextRow || beforeFirstDetailSection) {
			SelectCurrentSectionByTemplateSection (Report.DetailSection);
		} else {
			SelectCurrentSectionByTemplateSection (Report.ReportFooterSection);
		}
		beforeFirstDetailSection = false;
	}
	void AddControlsToCurrentPage (double span)
	{
		if (currentSection.SectionType != SectionType.PageFooter) {
			AddControlsToCurrentPage (span + spanCorrection, currentSectionControlsBuffer);
		} else {
			currentPageFooterSectionControlsBuffer.AddRange (currentSectionControlsBuffer);
			spanCorrection -= currentSection.Height;
		}
		
		if (subreportSectionControlsBuffer.Count > 0) {
			AddControlsToCurrentPage (span + spanCorrection, subreportSectionControlsBuffer);
		}
		currentSectionControlsBuffer.Clear ();
		subreportSectionControlsBuffer.Clear ();
	}
	void AddControlsToCurrentPage (double span, List<Control> controls)
	{
		foreach (var control in controls) {
			control.Top += span;
			currentPage.Controls.Add (control);
		}
	}
	void NextPage ()
	{
		AddControlsToCurrentPage (Report.Height - Report.PageFooterSection.Height, currentPageFooterSectionControlsBuffer);
		spanCorrection = 0;
		context.CurrentPageIndex++;
		currentPage = new Page { PageNumber = context.CurrentPageIndex };
		context.HeightLeftOnCurrentPage = Report.Height;
		context.HeightUsedOnCurrentPage = 0;
		currentPageFooterSectionControlsBuffer.Clear ();
		Report.Pages.Add (currentPage);
		SelectCurrentSectionByTemplateSection (Report.PageHeaderSection);
	}
	private void OnAfterReportProcess ()
	{
		//todo exec Report event
	}
}
internal struct SpanInfo
{
	internal double Treshold;
	internal double Span;
}

static internal class SectionExtensions
{
	public static IEnumerable<Control> GetCrossSectionControls (this Section section, Section endSection)
	{
		foreach (var c in section.Controls.Where (ctrl => ctrl is ICrossSectionControl)) {
			ICrossSectionControl csc = c as ICrossSectionControl;
			csc.StartSection = section;
			csc.EndSection = endSection;
			yield return c;
		}
	}
}