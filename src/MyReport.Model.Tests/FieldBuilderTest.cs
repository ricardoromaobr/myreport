using MyReport.Model.Data;

namespace MyReport.Tests;

public class FieldBuilderTest
{
    [Fact]
    public void CreateFields_ForClass_ReturnsFieldsWithAllProperties ()
    {
        TestClass tc = new TestClass();
        var fields = new List<Field>( FieldBuilder.CreateFields(tc,"kl",FieldKind.Data));
			
        Assert.Equal(4,fields.Count);
		
    }
		
    [Fact]
    public void CreateFields_ForString_ShouldCreateFieldWithName ()
    {
			 
        var fields = FieldBuilder.CreateFields(typeof(string),"sdf",FieldKind.Parameter);
			
        Assert.NotNull(fields);
        Assert.Equal(1,fields.Length);
        Assert.NotNull( fields[0].Name);
			
    }
		 
				
				
    public class TestClass {
        public string Name {
            get;
            set;
        }		
				
				
        public int Count {
            get;
            set;
        }
									
				
        public double Price {
            get;
            set;
        }	
				
        public DateTime Date {
            get;
            set;
        }					
				
    }
		
		
    public class InnerClass {
			
        public object Obj {
            get;
            set;
        }
    }
}