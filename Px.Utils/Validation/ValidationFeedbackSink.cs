using System.Collections.Concurrent;

namespace Px.Utils.Validation
{
    internal sealed class ValidationFeedbackSink
    {
        private const int DEFAULT_MAX_FEEDBACK_ITEMS_PER_SIGNATURE = 100;
        private readonly ConcurrentDictionary<ValidationFeedbackSignature, FeedbackBucket> _buckets = new(ValidationFeedbackSignatureComparer.Instance);
        private readonly int? _maxFeedbackItemsPerSignature;

        public ValidationFeedbackSink(ValidationOptions? options = null)
        {
            _maxFeedbackItemsPerSignature = options is null
                ? DEFAULT_MAX_FEEDBACK_ITEMS_PER_SIGNATURE
                : options.MaxFeedbackItemsPerSignature;
            if (_maxFeedbackItemsPerSignature is <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), "The maximum number of feedback items per signature must be positive or unlimited.");
            }
        }

        public void Report(KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> feedback)
            => Report(feedback.Key, feedback.Value);

        public void Report(ValidationFeedbackKey key, ValidationFeedbackValue value)
        {
            ValidationFeedbackSignature signature = new(value.Filename, key.Level, key.Rule);
            FeedbackBucket bucket = _buckets.GetOrAdd(signature, static _ => new FeedbackBucket());

            lock (bucket)
            {
                if (_maxFeedbackItemsPerSignature is null || bucket.Values.Count < _maxFeedbackItemsPerSignature.Value)
                {
                    bucket.Values.Add(value);
                    return;
                }

                if (!bucket.IsTruncated)
                {
                    ValidationFeedbackValue finalValue = bucket.Values[^1];
                    string truncationNote = $"Feedback limit of {_maxFeedbackItemsPerSignature.Value} instances for this file, level, and rule was reached. " +
                        $"Additional instances were detected but not logged.";
                    string additionalInfo = string.IsNullOrEmpty(finalValue.AdditionalInfo)
                        ? truncationNote
                        : $"{finalValue.AdditionalInfo}{Environment.NewLine}{truncationNote}";
                    bucket.Values[^1] = new ValidationFeedbackValue(finalValue.Filename, finalValue.Line, finalValue.Character, additionalInfo);
                    bucket.IsTruncated = true;
                }
            }
        }

        public void ReportRange(ValidationFeedback feedback)
        {
            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> feedbackGroup in feedback)
            {
                foreach (ValidationFeedbackValue value in feedbackGroup.Value)
                {
                    Report(feedbackGroup.Key, value);
                }
            }
        }

        public ValidationFeedback ToFeedback()
        {
            ValidationFeedback feedback = [];
            foreach (KeyValuePair<ValidationFeedbackSignature, FeedbackBucket> pair in _buckets)
            {
                lock (pair.Value)
                {
                    ValidationFeedbackKey key = new(pair.Key.Level, pair.Key.Rule);
                    if (!feedback.TryGetValue(key, out List<ValidationFeedbackValue>? values))
                    {
                        values = [];
                        feedback[key] = values;
                    }

                    values.AddRange(pair.Value.Values);
                }
            }

            return feedback;
        }

        private sealed class FeedbackBucket
        {
            public List<ValidationFeedbackValue> Values { get; } = [];

            public bool IsTruncated { get; set; }
        }
    }

    internal readonly record struct ValidationFeedbackSignature(string Filename, ValidationFeedbackLevel Level, ValidationFeedbackRule Rule);

    internal sealed class ValidationFeedbackSignatureComparer : IEqualityComparer<ValidationFeedbackSignature>
    {
        public static ValidationFeedbackSignatureComparer Instance { get; } = new();

        public bool Equals(ValidationFeedbackSignature x, ValidationFeedbackSignature y)
            => x.Level == y.Level && x.Rule == y.Rule && StringComparer.Ordinal.Equals(x.Filename, y.Filename);

        public int GetHashCode(ValidationFeedbackSignature obj)
            => HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Filename), obj.Level, obj.Rule);
    }
}
