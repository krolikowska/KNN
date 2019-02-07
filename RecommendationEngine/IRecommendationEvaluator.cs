using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine {
    public interface IRecommendationEvaluator {
        double EvaluateScoreForUSer(int userId, ISettings settings);
      
    }
}