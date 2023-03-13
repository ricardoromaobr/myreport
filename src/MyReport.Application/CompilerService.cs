using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CSharp;

namespace MyReport.Application;

public class CompilerService
{
    public CompilerService()
    {
    }

    public CompilerService(string codeTemplate) : this()
    {
        this.codeTemplate = codeTemplate;
    }

    LanguageCompiler cmp;

    public System.Collections.Specialized.StringCollection References
    {
        get { return cmp.Parms.ReferencedAssemblies; }
    }

    public bool Evaluate(out object result, out string message, object[] inputs)
    {
        cmp = new LanguageCompiler(LangType.CSharp, false, true, true);
        bool result_set = false;
        message = String.Empty;
        result = new object();
        try
        {
            cmp.Code = String.Format(CodeTemplate, inputs);
        }
        catch (Exception exp)
        {
            return false;
        }


        result_set = cmp.RunCode("GenerateDataSource", "Generate", new object[] { });
        if (result_set)
        {
            result = cmp.ResultObject;
            message = cmp.CompileErrors;
        }
        else
        {
            message = cmp.CompileErrors;
        }

        return result_set;
    }

    [Serializable]
    public enum LangType
    {
        CSharp,
        VBNet
    }

    string codeTemplate;

    public string CodeTemplate
    {
        get { return this.codeTemplate; }
        set { this.codeTemplate = value; }
    }

    public class LanguageCompiler
    {
        // Fields
        private string code;
        private StringBuilder compileErrors = new StringBuilder();
        private CompilerParameters parameters;

        // private CodeDomProvider provider = null;
        private CompilerResults results;
        public object ResultObject;
        private StringBuilder runErrors = new StringBuilder();

        private LangType language;

        // Methods
        public LanguageCompiler(LangType lang, bool generateExecutable, bool generateInMemory,
            bool includeDebugInformation)
        {
            language = lang;
            this.parameters = new CompilerParameters();
            this.parameters.GenerateExecutable = generateExecutable;
            this.parameters.GenerateInMemory = generateInMemory;
            this.parameters.IncludeDebugInformation = includeDebugInformation;
        }

        public void AddReferencedAssembly(string assembly)
        {
            this.parameters.ReferencedAssemblies.Add(assembly);
        }

        public bool CompileCode()
        {
            this.compileErrors = new StringBuilder();
            this.results = new NetLanguageProviderFactory().createLanguageCompiler(language)
                .CompileAssemblyFromSource(this.parameters, new string[] {this.code});
            if (this.results.Errors.Count > 0)
            {
                foreach (CompilerError error in this.results.Errors)
                {
                    this.compileErrors.Append(error.ToString() + System.Environment.NewLine + error.ErrorText);
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public void replaceTokens(string key, string value)
        {
            this.code = this.code.Replace(key, value);
        }

        public bool RunCode(string className, string methodName, object[] arguments)
        {
            if (this.CompileCode())
            {
                Type type = this.results.CompiledAssembly.GetType(className);
                object obj2 = this.results.CompiledAssembly.CreateInstance(className);
                try
                {
                    if ((type != null) && (obj2 != null))
                    {
                        ResultObject = type.GetMethod(methodName).Invoke(obj2, arguments);
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    this.runErrors.Append("error while running code " + className + System.Environment.NewLine +
                                          exception.ToString());
                }
            }
            else
            {
                this.runErrors.Append(this.CompileErrors);
            }

            return false;
        }


        public string Code
        {
            get { return this.code; }
            set { this.code = value; }
        }

        public string CompileErrors
        {
            get { return this.compileErrors.ToString(); }
        }

        public CompilerParameters Parms
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }

        public CompilerResults Results
        {
            get { return this.results; }
            set { this.results = value; }
        }

        public string RunErrors
        {
            get { return this.runErrors.ToString(); }
        }
    }

    public class NetLanguageProviderFactory
    {
        public CodeDomProvider createLanguageCompiler(LangType lang)
        {
            switch (lang)
            {
                case LangType.CSharp:
                    return new CSharpCodeProvider(new Dictionary<string, string>() {{"CompilerVersion", "v3.5"}});
            }

            return null;
        }
    }
}