using System;
using System.Collections.Generic;

public static class RecipeMatcher {
    public static bool IsExactMatch(IReadOnlyList<string> provided, IReadOnlyList<string> required) {
        if (provided == null || required == null) {
            return false;
        }

        if (provided.Count != required.Count) {
            return false;
        }

        Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < required.Count; i++) {
            string value = Normalize(required[i]);
            if (string.IsNullOrEmpty(value)) {
                continue;
            }

            if (!counts.ContainsKey(value)) {
                counts[value] = 0;
            }

            counts[value] += 1;
        }

        for (int i = 0; i < provided.Count; i++) {
            string value = Normalize(provided[i]);
            if (string.IsNullOrEmpty(value)) {
                continue;
            }

            int amount;
            if (!counts.TryGetValue(value, out amount)) {
                return false;
            }

            amount -= 1;
            if (amount < 0) {
                return false;
            }

            counts[value] = amount;
        }

        foreach (KeyValuePair<string, int> pair in counts) {
            if (pair.Value != 0) {
                return false;
            }
        }

        return true;
    }

    private static string Normalize(string value) {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
