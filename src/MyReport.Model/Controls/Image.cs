// 
// Image.cs
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

public class Image : Control, IResizable
{
    public Image() : base()
    {
        Border = new Border {WidthAll = 0, Color = new Color(0, 0, 0)};
        BackgroundColor = new Color(0.8, 0.8, 0.8);
        ImageKey = String.Empty;
    }

    public string ImageKey { get; set; }
    public Border Border { get; set; }

    public byte[] Data { get; set; }

    public override Control CreateControl()
    {
        Image img = new Image();
        CopyBasicProperties(img);
        img.ImageKey = ImageKey;
        img.Border = (Border) Border.Clone();
        img.Offset = new Point(offset.X, offset.Y);
        img.Data = Data;
        return img;
    }

    Point offset;

    public Point Offset
    {
        get { return offset; }
        set { offset = value; }
    }


    public bool CanGrow { get; set; }

    public bool CanShrink { get; set; }
}