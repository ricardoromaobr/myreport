// 
// Rectangle.cs
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

namespace MyReport.Model.Controls;

public class Line : Control
{
	public Line ():base()
	{			
		BackgroundColor =  new Color(0,0,0);			
		LineWidth = 2;
	}				
		
	public Point End { get; set; }
	public double LineWidth {get;set;}
	public bool ExtendToBottom {get;set;}
	public LineType LineType {get;set;}
	public LineMode LineMode {get;set;}
		
		
		 
	public override double Top
	{
		get
		{
			return Math.Min(Location.Y, End.Y);
		}set{
			if(Location.Y == End.Y){
				Location  = new Point(Location.X,value);
				End = new Point(End.X, value);
			} else if (Location.Y < End.Y) {
				End  = new Point(End.X, End.Y + (value - Location.Y));	
				Location  = new Point(Location.X,value);
									
			} else {
				Location  = new Point(Location.X, Location.Y + (value - End.Y));	
				End  = new Point(End.X,value);
			}
		}
          
	}
		
	public override double Height {
		get {
			return Bottom - Top + LineWidth;
		}
			 
	}
		
	public override double Width {
		get {
			if(this.Location.X  == this.End.X) {
				return Math.Max(this.Location.Y,  this.End.Y) - Math.Min(this.Location.Y,  this.End.Y);
			}
			else if(this.Location.Y == this.End.Y){
				return Math.Max(this.Location.X,  this.End.X) - Math.Min(this.Location.X,  this.End.X);
			}else {
				//TODO 3tk
				return 0;
			}
		}
			
	}
		
	public override double Left {
		get {
			return Math.Min(Location.X,End.X);
		}
		set {
			double val = Math.Min(Location.X,End.X) - value;
			Location = new Point(Location.X - val,Location.Y);
			End = new Point(End.X - val,End.Y);
		}
	}

	public override double Bottom
	{
		get
		{
			return Math.Max(Location.Y,End.Y) + LineWidth / 2;
		}
		set {
			double halfLineWidth = LineWidth / 2;
				
			if(Location.Y == End.Y){
				Location  = new Point(Location.X,value - halfLineWidth);
				End = new Point(End.X,  value - halfLineWidth);
			} else if (Location.Y < End.Y) {
				Location  = new Point(Location.X, Location.Y +  value - (End.Y + halfLineWidth));
				End  = new Point(End.X, value  - halfLineWidth);		
			} else {
				End  = new Point(End.X, End.Y + value - (Location.Y + halfLineWidth));	
				Location  = new Point(Location.X,value - halfLineWidth);
			}
		}
	}
		
	#region implemented abstract members of MonoReports.Model.Controls.Control
	public override Control CreateControl ()
	{
		Line line = new Line();
		CopyBasicProperties(line);			 
		line.End = new Point(End.X,End.Y);
		line.LineWidth = LineWidth;
		line.LineType = LineType;
		line.LineMode = line.LineMode;
		line.ExtendToBottom = ExtendToBottom;
		return line;
	}
		
	#endregion

         
}