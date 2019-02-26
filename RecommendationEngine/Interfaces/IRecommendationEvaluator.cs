using System.Collections.Generic;

namespace RecommendationEngine
{
    public interface IRecommendationEvaluator
    {
        double EvaluateScoreForUSer(int userId, ISettings settings);

        (double mae, double rsme) EvaluateScoreForUserWithErrorsEvaluation(int userId, ISettings settings, List<int> users);
    }
}