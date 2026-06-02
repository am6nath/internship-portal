namespace InternshipPortal.API.Helpers
{
    /// <summary>Local CS questions (algorithms, complexity, data structures) when OpenTDB is unavailable.</summary>
    public static class CsQuestionBank
    {
        public record CsQuestion(string Question, string CorrectAnswer, string[] IncorrectAnswers);

        public static readonly List<CsQuestion> Questions = new()
        {
            new("What is the time complexity of binary search on a sorted array of n elements?",
                "O(log n)", new[] { "O(n)", "O(n log n)", "O(1)" }),
            new("Which data structure uses LIFO (Last In, First Out)?",
                "Stack", new[] { "Queue", "Linked List", "Tree" }),
            new("What is the average time complexity of Quick Sort?",
                "O(n log n)", new[] { "O(n)", "O(n²)", "O(log n)" }),
            new("Which sorting algorithm has the best worst-case time complexity of O(n log n)?",
                "Merge Sort", new[] { "Bubble Sort", "Insertion Sort", "Quick Sort" }),
            new("What does 'Big O' notation describe?",
                "Upper bound of algorithm growth rate", new[] { "Exact runtime", "Memory address", "CPU cycles" }),
            new("Which traversal visits the root node between left and right subtrees?",
                "In-order", new[] { "Pre-order", "Post-order", "Level-order" }),
            new("What is the time complexity of accessing an element in an array by index?",
                "O(1)", new[] { "O(n)", "O(log n)", "O(n²)" }),
            new("Which data structure is best for implementing a priority queue?",
                "Heap", new[] { "Stack", "Array", "Hash Set" }),
            new("What is the space complexity of Merge Sort?",
                "O(n)", new[] { "O(1)", "O(log n)", "O(n²)" }),
            new("Which algorithm is used to find the shortest path in a weighted graph with non-negative edges?",
                "Dijkstra's Algorithm", new[] { "BFS", "DFS", "Kruskal's Algorithm" }),
            new("What is the worst-case time complexity of Bubble Sort?",
                "O(n²)", new[] { "O(n)", "O(n log n)", "O(log n)" }),
            new("Which data structure uses FIFO (First In, First Out)?",
                "Queue", new[] { "Stack", "Heap", "Graph" }),
            new("What is a hash collision?",
                "Two keys map to the same hash bucket", new[] { "Hash function failure", "Empty bucket", "Deleted entry" }),
            new("Which tree property ensures O(log n) search in a balanced BST?",
                "Height is logarithmic", new[] { "All nodes have two children", "Root is largest", "Leaves at same depth only" }),
            new("What is dynamic programming primarily used for?",
                "Solving overlapping subproblems optimally", new[] { "Parallel processing", "Memory allocation", "File compression" }),
            new("Which notation describes the tight bound of an algorithm?",
                "Theta (Θ)", new[] { "Omega (Ω) only", "Sigma (Σ)", "Delta (Δ)" }),
            new("What is the time complexity of inserting at the head of a singly linked list?",
                "O(1)", new[] { "O(n)", "O(log n)", "O(n log n)" }),
            new("Which graph traversal uses a queue?",
                "Breadth-First Search (BFS)", new[] { "Depth-First Search (DFS)", "Dijkstra's", "Prim's" }),
            new("What is the auxiliary space complexity of iterative Fibonacci with two variables?",
                "O(1)", new[] { "O(n)", "O(log n)", "O(n²)" }),
            new("Which data structure supports O(1) average lookup, insert, and delete?",
                "Hash Table", new[] { "Binary Search Tree", "Sorted Array", "Linked List" }),
            new("What is the time complexity of building a max-heap from an unsorted array?",
                "O(n)", new[] { "O(n log n)", "O(n²)", "O(log n)" }),
            new("Which algorithm design paradigm divides a problem into independent subproblems?",
                "Divide and Conquer", new[] { "Greedy", "Backtracking", "Branch and Bound" }),
            new("What is the best-case time complexity of Insertion Sort?",
                "O(n)", new[] { "O(n²)", "O(n log n)", "O(log n)" }),
            new("Which structure is used to detect cycles in a linked list efficiently?",
                "Floyd's Tortoise and Hare", new[] { "Stack only", "Queue only", "Hash map only" }),
            new("What is amortized O(1) insertion referring to in dynamic arrays?",
                "Average constant time per insert over many operations", new[] { "Always one operation", "Worst case always O(1)", "No resizing ever" })
        };

        public static List<(string Id, string Question, List<string> Options, string CorrectAnswer)> GetRandomQuestions(int count, int seed)
        {
            var rng = new Random(seed);
            var selected = Questions.OrderBy(_ => rng.Next()).Take(count).ToList();
            var result = new List<(string, string, List<string>, string)>();

            for (var i = 0; i < selected.Count; i++)
            {
                var q = selected[i];
                var options = new List<string> { q.CorrectAnswer };
                options.AddRange(q.IncorrectAnswers);
                options = options.OrderBy(_ => rng.Next()).ToList();
                result.Add(($"cs{i + 1}", q.Question, options, q.CorrectAnswer));
            }

            return result;
        }
    }
}
