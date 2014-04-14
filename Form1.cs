using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AnagramFinder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private String genKey(String str)
        {
            return new string(str.ToCharArray().OrderBy(s => s).ToArray());
        }

        private bool checkAnagram(String str1, String str2)
        {
            char[] ca1 = str1.ToCharArray();
            char[] ca2 = str2.ToCharArray();

    
            // Check that the 2 words are fully scrambled anagrams
            for (int i = 0; i < ca1.Length; i++)
            {
                for (int j = 0; j < ca2.Length; j++)
                {
                    if (ca1[i] == ca2[j])
                    {
                        // found matching characters
                        if (i == ca1.Length - 1 && j < ca2.Length-1) continue; // char @ i is last character whereas the one @j isn't.
                        if (j == ca2.Length - 1 && i < ca1.Length-1) continue; // char @ j is last character whereas the one @i isn't.
                        if (i == ca1.Length - 1 && j == ca2.Length - 1) continue; // char @ i and char @ j are both last. this is ok.
                        if (ca1[i + 1] == ca2[j + 1]) return false; // anagram is not fully scrambled
                    }
                }
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
            textBox1.SelectAll();
            button2.Enabled = true;
            textBox2.Clear();
        }

        private void disableControls()
        {
            button1.Enabled = false;
            textBox1.Enabled = false;
            button2.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            numericUpDown1.Enabled = false;
            label1.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
        }

        private void enableControls()
        {
            button1.Enabled = true;
            textBox1.Enabled = true;
            button2.Enabled = true;
            checkBox1.Enabled = true;
            checkBox2.Enabled = true;
            numericUpDown1.Enabled = true;
            label1.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            disableControls();

            // Dictionary to hold our words
            Dictionary<String,String> dict=new Dictionary<string,string>();
            int longestAnagramLength = 0;
            string filename = textBox1.Text;
            int minLen=Convert.ToInt32(numericUpDown1.Value);
            bool showMin = checkBox1.Checked;
            bool reqScram = checkBox2.Checked;
            int anagramsFound = 0;
            string longest1 = null, longest2 = null;

            int mostAnagramedAmount = 0;
            string mostAnagramedKey = null;

            // Note the starting time of this operation
            long opStartTime = DateTime.UtcNow.Ticks;

            // Begin displaying stuff
            textBox2.Clear();
            textBox2.AppendText("Attempting to read file "+filename+"...\r\n");

            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    String word;

                    while ((word = sr.ReadLine()) != null)
                    {
                        // read the word, convert to uppercase
                        word = word.ToUpper();

                        // Ignore words less than 3 characters long
                        if (word.Length < 3)
                            continue;

                        // generate key from the word
                        string key=genKey(word);

                        // key index (1-indexed)
                        int ki = 1;
                        int ki2 = 1; // this is needed for tmes when the user has clicked the "require words to be completely scrambled" checkbox

                        // generate numbered key
                        string nkey = key + ki;

                        // Variable to check if the word is already in the dict
                        bool alreadyThere = false;

                        // Assume anagram
                        bool anagrams = true;

                        while(dict.ContainsKey(nkey))
                        {
                            // check if the word is already there
                            if (word == dict[nkey])
                            {
                                alreadyThere = true;
                                break;
                            }


                            // check if it's an anagram
                            bool anagram = reqScram ? checkAnagram(dict[nkey], word) : true;


                            if (!anagram)
                            {
                                // try the next key
                                nkey = key + (++ki);
                                anagrams = false;
                                continue;
                            }

                            // see if we've found a most anagramed word
                            if (ki2 > mostAnagramedAmount || (ki2==mostAnagramedAmount && word.Length > mostAnagramedKey.Length))
                            {
                                mostAnagramedAmount = ki2;
                                mostAnagramedKey = key;
                            }

                            // see if we've found a longest anagram
                            if (key.Length > longestAnagramLength)
                            {
                                longestAnagramLength = key.Length;
                                longest1 = word;
                                longest2 = dict[nkey];
                            }
                            
                            // print the anagram if its above a certain length
                            if (showMin && key.Length >= minLen)
                            {
                                textBox2.AppendText(word+" is an anagram of "+dict[nkey]+"("+key.Length+")\r\n");
                            }

                            // generate next numbered key
                            nkey = key + (++ki);
                            ki2++;
                        }

                        if (!alreadyThere && anagrams)
                        {
                            // add this as another key
                            dict.Add(nkey, word);

                            // We have a new anagram
                            if (ki == 2)
                            {
                                anagramsFound++;
                                label3.Text = anagramsFound.ToString();
                            }
                        }
                    }

                    if (longest1 != null && longest2 != null)
                    {
                        textBox2.AppendText("Longest anagram found : "+longest1+" / "+longest2+" ("+longestAnagramLength+")\r\n");
                    }
                }// using

                if (mostAnagramedAmount > 0)
                {
                    mostAnagramedAmount++;
                    // Display the most anagramed key.
                    textBox2.AppendText("The most anagramed key is " + mostAnagramedKey + " with " + mostAnagramedAmount + " anagrams :");
                    for (int i = 1; i <= mostAnagramedAmount; i++)
                    {
                        textBox2.AppendText(dict[mostAnagramedKey + i]);
                        if (i < mostAnagramedAmount) textBox2.AppendText(", ");
                    }
                }

                textBox2.AppendText("\r\n");
            }
            catch (Exception ex)
            {
                textBox2.AppendText(ex.Message);
            }
            finally
            {
                textBox2.AppendText("Operation took "+((decimal)DateTime.UtcNow.Ticks-opStartTime)/(decimal)10000000+" secs");
                enableControls();
            }
        }
    }
}
