namespace CStone;
public class ListUtils
{
    private readonly IList<Tuple<string, int>> _list;

    public ListUtils(IList<Tuple<string, int>> list)
    {
        _list = list;
    }

    public ConfidenceResult GetConfidence(int minSamples, float MinConfidence)
    {
        if (_list.Count > 0)
        {
            var most = from i in _list
                       group i by i into grp
                       orderby grp.Count() descending
                       select new ConfidenceResult(grp.Key.Item1, grp.Key.Item2, grp.Count(), grp.Count() / _list.Count);

            var result = most.First();
                    
            // Adjust confidence based on sample size
            if (result.Count < minSamples) {
                result.Confidence = result.Confidence / (minSamples / result.Count);
            }

            return result;
        }
        return new ConfidenceResult();
    }
}