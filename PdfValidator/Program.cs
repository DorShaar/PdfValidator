using PdfValidator.Infrastracture;
using System;
using System.IO;

namespace PdfValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please insert pdf path");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Path {args[0]} does not exist");
                return;
            }

            string filePath = args[0];
            IPdfValidator fileParser = new FileParser(new LineParser());

            //using (TimeTracer.TimeTracer timeTracer = new TimeTracer.TimeTracer("String parsing"))
            //{
            //    List<ObjectData> objects = fileParser.Parse(args[0]).Result;
            //}

            //fileParser = new FileParser(new LineParserSpans());
            //using (TimeTracer.TimeTracer timeTracer = new TimeTracer.TimeTracer("Spans parsing"))
            //{
            //    List<ObjectData> objects = fileParser.Parse(args[0]).Result;
            //}

            fileParser = new FileParserSpansAndPipes();
            using (TimeTracer.TimeTracer timeTracer = new TimeTracer.TimeTracer("Spans and pipes parsing"))
            {
                ValidationResult validationResult = fileParser.Validate(filePath).Result;
                if(validationResult.Result == true)
                    Console.WriteLine($"{filePath} validation success!");
                else
                    Console.WriteLine($"{filePath} validation failed with the next error: {validationResult.ErrorsText}");
            }
        }
    }
}
