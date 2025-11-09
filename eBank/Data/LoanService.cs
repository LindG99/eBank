using Microsoft.ML;
using Microsoft.ML.Data;

namespace eBank.Data
{
    
    public class LoanData
    {
        public float Income;
        public float Debt;
        public float CreditScore;
        public float EmploymentYears;
        public float LoanAmount;
        public bool Approved; // dummy, Needed for the ML model input
    }

    public class LoanPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Approved;
        public float Probability;
    }

    
    public class LoanService
    {
        private static readonly MLContext _mlContext = new MLContext();
        private static readonly ITransformer _mlModel;
        private static readonly PredictionEngine<LoanData, LoanPrediction> _engine;

        // Static constructor to load the ML model once
        static LoanService()
        {
            var modelPath = "App_Data/LoanModel.zip";

            if (!System.IO.File.Exists(modelPath))
                Console.WriteLine($"[LoanService] Fel: Filen hittades inte: {modelPath}");
            else
                Console.WriteLine("[LoanService] Filen hittad, laddar modell...");

            _mlModel = _mlContext.Model.Load(modelPath, out _);
            _engine = _mlContext.Model.CreatePredictionEngine<LoanData, LoanPrediction>(_mlModel);

            Console.WriteLine("[LoanService] Modell laddad klart (statisk)");
        }
        // Method to evaluate loan application
        public LoanPrediction Evaluate(LoanData input)
        {
            Console.WriteLine("[LoanService] Gör prediktion...");
            var result = _engine.Predict(input);
            Console.WriteLine($"[LoanService] Prediktion klar: Approved={result.Approved}, Probability={result.Probability}");
            return result;
        }
    }
}
