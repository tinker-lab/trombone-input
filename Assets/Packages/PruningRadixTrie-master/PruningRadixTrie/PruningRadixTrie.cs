﻿using System;
using System.Diagnostics;
using System.Collections.Generic;

// Written by wolf Garbe in PruningRadixTrie on Github, designd for autocomplete
namespace Auto
{
    public class PruningRadixTrie
    {
        public long termCount = 0;
        public long termCountLoaded = 0;

        //Trie node class
        public class Node
        {
            public List<NamedNode> Children;

            //Does this node represent the last character in a word? 
            //0: no word; >0: is word (termFrequencyCount)
            public long termFrequencyCount;
            public long termFrequencyCountChildMax;

            public Node(long termfrequencyCount)
            {
                termFrequencyCount = termfrequencyCount;
            }
        }

        public struct NamedNode
        {
            public string key;
            public Node node;

            public NamedNode(string key, Node node)
            {
                this.key = key;
                this.node = node;
            }
        }

        public struct TermFreq
        {
            public string term;
            public long termFrequencyCount;

            public TermFreq(string term, long termFrequencyCount)
            {
                this.term = term;
                this.termFrequencyCount = termFrequencyCount;
            }
        }

        //The trie
        private readonly Node trie;

        public PruningRadixTrie()
        {
            trie = new Node(0);
        }

        // Insert a word into the trie
        public void AddTerm(String term, long termFrequencyCount)
        {
            List<Node> nodeList = new List<Node>();
            AddTerm(trie, term, termFrequencyCount, 0, 0, nodeList);
        }

        public void UpdateMaxCounts(List<Node> nodeList, long termFrequencyCount)
        {
            foreach (Node node in nodeList) if (termFrequencyCount > node.termFrequencyCountChildMax) node.termFrequencyCountChildMax = termFrequencyCount;
        }

        public void AddTerm(Node curr, String term, long termFrequencyCount, int id, int level, List<Node> nodeList)
        {
            try
            {
                nodeList.Add(curr);

                //test for common prefix (with possibly different suffix)
                int common = 0;
                if (curr.Children != null)
                {
                    for (int j = 0; j < curr.Children.Count; j++)
                    {
                        NamedNode nnode = curr.Children[j];
                        var key = nnode.key;
                        var node = nnode.node;

                        for (int i = 0; i < Math.Min(term.Length, key.Length); i++) if (term[i] == key[i]) common = i + 1; else break;

                        if (common > 0)
                        {
                            //term already existed
                            //existing ab
                            //new      ab
                            if ((common == term.Length) && (common == key.Length))
                            {
                                if (node.termFrequencyCount == 0) termCount++;
                                node.termFrequencyCount += termFrequencyCount;
                                UpdateMaxCounts(nodeList, node.termFrequencyCount);
                            }
                            //new is subkey
                            //existing abcd
                            //new      ab
                            //if new is shorter (== common), then node(count) and only 1. children add (clause2)
                            else if (common == term.Length)
                            {
                                //insert second part of oldKey as child 
                                Node child = new Node(termFrequencyCount);
                                child.Children = new List<NamedNode>
                                {
                                   new NamedNode(key.Substring(common), node)
                                };
                                child.termFrequencyCountChildMax = Math.Max(node.termFrequencyCountChildMax, node.termFrequencyCount);
                                UpdateMaxCounts(nodeList, termFrequencyCount);

                                //insert first part as key, overwrite old node
                                curr.Children[j] = new NamedNode(term.Substring(0, common), child);
                                //sort children descending by termFrequencyCountChildMax to start lookup with most promising branch
                                curr.Children.Sort((x, y) => y.node.termFrequencyCountChildMax.CompareTo(x.node.termFrequencyCountChildMax));
                                //increment termcount by 1
                                termCount++;
                            }
                            //if oldkey shorter (==common), then recursive addTerm (clause1)
                            //existing: te
                            //new:      test
                            else if (common == key.Length)
                            {
                                AddTerm(node, term.Substring(common), termFrequencyCount, id, level + 1, nodeList);
                            }
                            //old and new have common substrings
                            //existing: test
                            //new:      team
                            else
                            {
                                //insert second part of oldKey and of s as child 
                                Node child = new Node(0);//count       
                                child.Children = new List<NamedNode>
                                {
                                     new NamedNode(key.Substring(common), node) ,
                                     new NamedNode(term.Substring(common), new Node(termFrequencyCount))
                                };
                                child.termFrequencyCountChildMax = Math.Max(node.termFrequencyCountChildMax, Math.Max(termFrequencyCount, node.termFrequencyCount));
                                UpdateMaxCounts(nodeList, termFrequencyCount);

                                //insert first part as key. overwrite old node
                                curr.Children[j] = new NamedNode(term.Substring(0, common), child);
                                //sort children descending by termFrequencyCountChildMax to start lookup with most promising branch
                                curr.Children.Sort((x, y) => y.node.termFrequencyCountChildMax.CompareTo(x.node.termFrequencyCountChildMax));
                                //increment termcount by 1 
                                termCount++;
                            }
                            return;
                        }
                    }
                }

                // initialize dictionary if first key is inserted 
                if (curr.Children == null)
                {
                    curr.Children = new List<NamedNode>
                        {
                            new NamedNode( term, new Node(termFrequencyCount) )
                        };
                }
                else
                {
                    curr.Children.Add(new NamedNode(term, new Node(termFrequencyCount)));
                    //sort children descending by termFrequencyCountChildMax to start lookup with most promising branch
                    curr.Children.Sort((x, y) => y.node.termFrequencyCountChildMax.CompareTo(x.node.termFrequencyCountChildMax));
                }
                termCount++;
                UpdateMaxCounts(nodeList, termFrequencyCount);
            }
            catch (Exception e) { Console.WriteLine("exception: " + term + " " + e.Message); }
        }

        public void FindAllChildTerms(String prefix, int topK, ref long termFrequencyCountPrefix, string prefixString, List<TermFreq> results, bool pruning)
        {
            FindAllChildTerms(prefix, trie, topK, ref termFrequencyCountPrefix, prefixString, results, null, pruning);
        }

        public void FindAllChildTerms(String prefix, Node curr, int topK, ref long termfrequencyCountPrefix, string prefixString, List<TermFreq> results, System.IO.StreamWriter file, bool pruning)
        {
            try
            {
                //pruning/early termination in radix trie lookup
                if (pruning && (topK > 0) && (results.Count == topK) && (curr.termFrequencyCountChildMax <= results[topK - 1].termFrequencyCount)) return;

                //test for common prefix (with possibly different suffix)
                bool noPrefix = string.IsNullOrEmpty(prefix);

                if (curr.Children != null)
                {
                    foreach (NamedNode nnode in curr.Children)
                    {
                        var key = nnode.key;
                        var node = nnode.node;
                        //pruning/early termination in radix trie lookup
                        if (pruning && (topK > 0) && (results.Count == topK) && (node.termFrequencyCount <= results[topK - 1].termFrequencyCount) && (node.termFrequencyCountChildMax <= results[topK - 1].termFrequencyCount))
                        {
                            if (!noPrefix) break; else continue;
                        }

                        if (noPrefix || key.StartsWith(prefix))
                        {
                            if (node.termFrequencyCount > 0)
                            {
                                if (prefix == key) termfrequencyCountPrefix = node.termFrequencyCount;

                                //candidate                              
                                if (file != null) file.WriteLine(prefixString + key + "\t" + node.termFrequencyCount.ToString());
                                else
                                if (topK > 0) AddTopKSuggestion(prefixString + key, node.termFrequencyCount, topK, ref results); else results.Add(new TermFreq(prefixString + key, node.termFrequencyCount));
                            }

                            if ((node.Children != null) && (node.Children.Count > 0)) FindAllChildTerms("", node, topK, ref termfrequencyCountPrefix, prefixString + key, results, file, pruning);
                            if (!noPrefix) break;
                        }
                        else if (prefix.StartsWith(key))
                        {

                            if ((node.Children != null) && (node.Children.Count > 0)) FindAllChildTerms(prefix.Substring(key.Length), node, topK, ref termfrequencyCountPrefix, prefixString + key, results, file, pruning);
                            break;
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine("exception: " + prefix + " " + e.Message); }
        }

        public List<TermFreq> GetTopkTermsForPrefix(String prefix, int topK, out long termFrequencyCountPrefix, bool pruning = true)
        {
            List<TermFreq> results = new List<TermFreq>();

            //termFrequency of prefix, if it exists in the dictionary (even if not returned in the topK results due to low termFrequency)
            termFrequencyCountPrefix = 0;

            // At the end of the prefix, find all child words
            FindAllChildTerms(prefix, topK, ref termFrequencyCountPrefix, "", results, pruning);

            return results;
        }


        public void WriteTermsToFile(string path)
        {
            //save only if new terms were added
            if (termCountLoaded == termCount) return;
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
                {
                    long prefixCount = 0;
                    FindAllChildTerms("", trie, 0, ref prefixCount, "", null, file, true);
                }
                Console.WriteLine(termCount.ToString("N0") + " terms written.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Writing terms exception: " + e.Message);
            }
        }

        public bool ReadTermsFromFrequencyDictionary(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("Could not find file " + path);
                return false;
            }

            try
            {
                using (System.IO.Stream corpusStream = System.IO.File.OpenRead(path))
                {
                    ReadTermsFromStream(corpusStream, '\t');
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Loading terms exception: " + e.Message);
            }

            return true;
        }

        // ripped from ReadTermsFromFrequencyDictionary(string) by Logan to allow Unity Resource Manager to provide stream
        // requires the format (.*?)<seperator>(\d+\s*)\n for valid lines, running AddTerm($1, Int64.TryParse($2))
        public void ReadTermsFromStream(System.IO.Stream corpusStream, char separator)
        {
            Stopwatch sw1 = Stopwatch.StartNew();

            using (System.IO.StreamReader sr = new System.IO.StreamReader(corpusStream, System.Text.Encoding.UTF8, false))
            {
                String line;

                //process a single line at a time only for memory efficiency
                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineParts = line.Split(separator);
                    if (lineParts.Length == 2)
                    {
                        Int64 count;
                        if (Int64.TryParse(lineParts[1], out count))
                        {
                            this.AddTerm(lineParts[0], count);
                        }
                    }
                }

            }

            termCountLoaded = termCount;
            Console.WriteLine($"{termCount.ToString("N0")} terms loaded in {sw1.ElapsedMilliseconds.ToString("N0")} ms");
        }

        public class BinarySearchComparer : IComparer<TermFreq>
        {
            public int Compare(TermFreq f1, TermFreq f2)
            {
                return Comparer<long>.Default.Compare(f2.termFrequencyCount, f1.termFrequencyCount);//descending
            }
        }

        public void AddTopKSuggestion(string term, long termFrequencyCount, int topK, ref List<TermFreq> results)
        {
            //at the end/highest index is the lowest value
            // >  : old take precedence for equal rank   
            // >= : new take precedence for equal rank 
            if ((results.Count < topK) || (termFrequencyCount >= results[topK - 1].termFrequencyCount))
            {
                int index = results.BinarySearch(new TermFreq(term, termFrequencyCount), new BinarySearchComparer());
                if (index < 0) results.Insert(~index, new TermFreq(term, termFrequencyCount)); else results.Insert(index, new TermFreq(term, termFrequencyCount));

                if (results.Count > topK) results.RemoveAt(topK);
            }

        }

    }
}
