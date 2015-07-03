using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tribit
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: There are on input arguments.");
                Console.ReadLine();
                return;
            }

            String sSource = args[0];
            Console.WriteLine("tribit.exe {0}", sSource);

            // Input data checking. If input data is invalid, then exit
            if (!CheckSourceData(sSource))
            {
                Console.ReadLine();
                return;
            }

            // In case of 1 cell pyramid just print it and exit
            if (sSource.Length == 1)
            {
                Console.WriteLine("{0}", sSource);
                Console.ReadLine();
                return;
            }

            // Initialize entire pyramid
            PyramidClass Pyramid = new PyramidClass(sSource);

            // Print this pyramid
            Console.WriteLine("{0}", Pyramid.GetString());

            FourCellPyramid TopPyramid = Pyramid.TopPyramid;
            // Transition iteration process
            while (true)
            {
                // Make transition step
                TopPyramid.DoTransition();

                // Print pyramid on current step
                Console.WriteLine("{0}", Pyramid.GetString());

                // Check pyramid for posibility of reducing it
                if (TopPyramid.IsReducingPossible())
                {
                    // if reduce possible, do it
                    TopPyramid.Reduce(true);

                    // the changes
                    // after reducing of the pyramid we must redefine line levels for cells
                    TopPyramid.DefLineLevels(0);

                    // Print pyramid just after reducing
                    Console.WriteLine("{0}", Pyramid.GetString());

                }

                // if pattern of root pyramid is definded, then reduce this root pyramid and finish algorithm
                if ((TopPyramid.PyramidLevel <= 1) && TopPyramid.IsReducingPossible())
                {
                    int value = TopPyramid.TopValue ? 1 : 0;
                    Console.WriteLine("{0}", value);
                    break;
                }
            }

            Console.ReadLine();
            return;
        }

        static private bool CheckSourceData(String sSource)
        {
            string[] correctDigits = { "0", "1" };
            bool sourceCorrect = true;
            int len = sSource.Length;

            // - on correct length
            int pow = 0; // Power of 4 for checking input data length
            while (sourceCorrect)
            {
                if (len == Math.Pow(4, pow))
                    break;

                if (len < Math.Pow(4, pow))
                {
                    Console.WriteLine("Error: Incorrect input string length");
                    sourceCorrect = false;
                    break;
                }
                pow++;
            }

            // - on connect content
            int charPos = 0;

            while (charPos < len)
            {
                if (!(correctDigits.Contains(sSource.Substring(charPos, 1))))
                {
                    Console.WriteLine("Error: Invalid character in input string");
                    sourceCorrect = false;
                    break;
                }
                charPos++;
            }
            return sourceCorrect;
        }
    }

    public class PyramidClass
    {
        public int MaxLines; // The number of strings in resulting pyramid
        public int MaxPyrLevel; // The number of levels of the pyramid (4 cells - level = 1)
        public FourCellPyramid TopPyramid;
        String source;
        public PyramidClass(String aSource)
        {
            source = aSource;
            int len = source.Length;
            MaxLines = 1;
            MaxPyrLevel = 0;
            while (len > 1)
            {
                len = len / 4;
                MaxLines *= 2;
                MaxPyrLevel++;
            }
            MaxLines--;

            // Initializing top 4 cell pyramid
            TopPyramid = new FourCellPyramid(null, MaxPyrLevel, false);

            // defining line levels cells
            TopPyramid.DefLineLevels(0);

            // Filling pyramid from input string
            FillFromString(aSource);
        }

        public void FillFromString(String aSource)
        {
            int level = 0;
            String s = aSource;
            while (level <= MaxLines)
            {
                s = TopPyramid.FillFromString(s, level++);
            }
        }

        public String GetString()
        {
            int level = 0;
            String s = "";
            String sLevelString = "";
            while (level <= MaxLines)
            {
                sLevelString = TopPyramid.GetString(level++);
                if (sLevelString.Length > 0)
                    //                    s = sLevelString + " " + s;
                    s = sLevelString + s;
            }
            return s;
        }
    }

    public class FourCellPyramid
    {
        // 4 cell pyramid values
        // false - 0 (white)
        // true - 1 (black)
        public bool TopValue;
        public bool BottomLeftValue;
        public bool BottomUpsideValue;
        public bool BottomRightValue;

        public bool Upside; // Is this pyramid upside
        public int PyramidLevel; // Level of pyramid in structure
        public int TopLineLevel; // Global pyramid level of top value
        public int BottomLineLevel; // Global pyramid level of bottom values

        FourCellPyramid ParentPyramid;
        FourCellPyramid Top4CellPyramid;
        FourCellPyramid BottomLeft4CellPyramid;
        FourCellPyramid BottomUpside4CellPyramid;
        FourCellPyramid BottomRight4CellPyramid;

        public FourCellPyramid(FourCellPyramid aParent, int aPypamidPyrLevel, bool aUpside)
        {
            ParentPyramid = aParent;
            PyramidLevel = aPypamidPyrLevel;

            TopValue = false;
            BottomLeftValue = false;
            BottomUpsideValue = false;
            BottomRightValue = false;
            Upside = aUpside;
            TopLineLevel = 0;
            BottomLineLevel = 0;

            if (aPypamidPyrLevel > 1)
            {

                Top4CellPyramid = new FourCellPyramid(this, aPypamidPyrLevel - 1, false);
                BottomLeft4CellPyramid = new FourCellPyramid(this, aPypamidPyrLevel - 1, false);
                BottomUpside4CellPyramid = new FourCellPyramid(this, aPypamidPyrLevel - 1, true);
                BottomRight4CellPyramid = new FourCellPyramid(this, aPypamidPyrLevel - 1, false); ;
            }
            else
            {
                Top4CellPyramid = null;
                BottomLeft4CellPyramid = null;
                BottomUpside4CellPyramid = null;
                BottomRight4CellPyramid = null;
            }
        }

        public bool GetActualUpside()
        {
            if (ParentPyramid != null)
                return ParentPyramid.GetActualUpside() ^ Upside;
            else
                return Upside;
        }

        public void DefLineLevels(int aBaseLevel)
        {
            // if it's 0-level (values containing) pyramid set next free line level to TopLineLevel
            if (Top4CellPyramid == null)
            {
                TopLineLevel = aBaseLevel;
                BottomLineLevel = aBaseLevel + 1;
            }
            else
            {
                int BottomPartBaseLevel = aBaseLevel + (int)Math.Pow(2, PyramidLevel - 1);
                if (GetActualUpside())
                {
                    BottomLeft4CellPyramid.DefLineLevels(aBaseLevel);
                    BottomUpside4CellPyramid.DefLineLevels(aBaseLevel);
                    BottomRight4CellPyramid.DefLineLevels(aBaseLevel);
                    Top4CellPyramid.DefLineLevels(BottomPartBaseLevel);
                }
                else
                {
                    Top4CellPyramid.DefLineLevels(aBaseLevel);
                    BottomLeft4CellPyramid.DefLineLevels(BottomPartBaseLevel);
                    BottomUpside4CellPyramid.DefLineLevels(BottomPartBaseLevel);
                    BottomRight4CellPyramid.DefLineLevels(BottomPartBaseLevel);
                }
            }
        }


        public String FillFromString(String aSource, int aLevel)
        {
            String s = aSource;

            if (Top4CellPyramid == null)
            {
                // if correct levels, set corresponding valuse of this 4 cell pyramid 
                if ((aLevel == TopLineLevel && !GetActualUpside()) || (aLevel == BottomLineLevel && GetActualUpside()))
                {
                    TopValue = (s.Substring(s.Length - 1, 1) == "1");
                    s = s.Substring(0, s.Length - 1);
                }

                if ((aLevel == BottomLineLevel && !GetActualUpside()) || (aLevel == TopLineLevel && GetActualUpside()))
                {
                    BottomLeftValue = (s.Substring(s.Length - 1, 1) == "1");
                    s = s.Substring(0, s.Length - 1);
                    BottomUpsideValue = (s.Substring(s.Length - 1, 1) == "1");
                    s = s.Substring(0, s.Length - 1);
                    BottomRightValue = (s.Substring(s.Length - 1, 1) == "1");
                    s = s.Substring(0, s.Length - 1);
                }
            }
            else
            {
                // Then give sibling pyramids to process rest data
                if (Top4CellPyramid != null)
                    s = Top4CellPyramid.FillFromString(s, aLevel);
                if (BottomLeft4CellPyramid != null)
                    s = BottomLeft4CellPyramid.FillFromString(s, aLevel);
                if (BottomUpside4CellPyramid != null)
                    s = BottomUpside4CellPyramid.FillFromString(s, aLevel);
                if (BottomRight4CellPyramid != null)
                    s = BottomRight4CellPyramid.FillFromString(s, aLevel);
            }
            // return source data, unprocessed on this step
            return s;
        }

        public String GetString(int aLevel)
        {
            String s = "";
            if (Top4CellPyramid == null)
            {
                // if correct levels, set corresponding valuse of this 4 cell pyramid 
                if ((aLevel == TopLineLevel && !GetActualUpside()) || (aLevel == BottomLineLevel && GetActualUpside()))
                    s = (TopValue ? "1" : "0") + s;
                if ((aLevel == BottomLineLevel && !GetActualUpside()) || (aLevel == TopLineLevel && GetActualUpside()))
                    s = (BottomRightValue ? "1" : "0") +
                        (BottomUpsideValue ? "1" : "0") +
                        (BottomLeftValue ? "1" : "0") + s;
            }
            else
            {
                if (Top4CellPyramid != null)
                    s = Top4CellPyramid.GetString(aLevel) + s;
                if (BottomLeft4CellPyramid != null)
                    s = BottomLeft4CellPyramid.GetString(aLevel) + s;
                if (BottomUpside4CellPyramid != null)
                    s = BottomUpside4CellPyramid.GetString(aLevel) + s;
                if (BottomRight4CellPyramid != null)
                    s = BottomRight4CellPyramid.GetString(aLevel) + s;
            }

            // return source data, unprocessed on this step
            return s;
        }


        public String GetStringRepresentation()
        {
            String s = "";
            if (Top4CellPyramid == null)
                s = (BottomRightValue ? "1" : "0") +
                    (BottomUpsideValue ? "1" : "0") +
                    (BottomLeftValue ? "1" : "0") +
                    (TopValue ? "1" : "0");
            return s;
        }

        public void SetStringRepresentation(String aStringRepresentation)
        {
            if ((Top4CellPyramid == null) && (aStringRepresentation.Length == 4))
            {
                BottomRightValue = (aStringRepresentation.Substring(0, 1) == "1");
                BottomUpsideValue = (aStringRepresentation.Substring(1, 1) == "1");
                BottomLeftValue = (aStringRepresentation.Substring(2, 1) == "1");
                TopValue = (aStringRepresentation.Substring(3, 1) == "1");
            }
        }

        public void DoTransition()
        {
            // if this is value-containing pyramid, process the transition
            // else process the transition of sibling pyramids
            if (Top4CellPyramid == null)
            {
                // Do transition of this pyramid
                String sPattern = GetStringRepresentation().Substring(0, 4);
                String newPattern = "0000";
                if (sPattern.Equals("0000")) newPattern = "0000";
                if (sPattern.Equals("0001")) newPattern = "1000";
                if (sPattern.Equals("0010")) newPattern = "0001";
                if (sPattern.Equals("0011")) newPattern = "0010";
                if (sPattern.Equals("0100")) newPattern = "0000";
                if (sPattern.Equals("0101")) newPattern = "0010";
                if (sPattern.Equals("0110")) newPattern = "1011";
                if (sPattern.Equals("0111")) newPattern = "1011";
                if (sPattern.Equals("1000")) newPattern = "0100";
                if (sPattern.Equals("1001")) newPattern = "0101";
                if (sPattern.Equals("1010")) newPattern = "0111";
                if (sPattern.Equals("1011")) newPattern = "1111";
                if (sPattern.Equals("1100")) newPattern = "1101";
                if (sPattern.Equals("1101")) newPattern = "1110";
                if (sPattern.Equals("1110")) newPattern = "0111";
                if (sPattern.Equals("1111")) newPattern = "1111";
                SetStringRepresentation(newPattern); // save transitioned value back to the pyramid
            }
            else
            {
                if (Top4CellPyramid != null)
                    Top4CellPyramid.DoTransition();
                if (BottomLeft4CellPyramid != null)
                    BottomLeft4CellPyramid.DoTransition();
                if (BottomUpside4CellPyramid != null)
                    BottomUpside4CellPyramid.DoTransition();
                if (BottomRight4CellPyramid != null)
                    BottomRight4CellPyramid.DoTransition();
            }
        }

        // Get max possible reduce group level
        // true - reducing possible in base pyramids level
        // false - reducing is not possible
        public bool IsReducingPossible()
        {
            bool res = true;
            // if we have sub pyramides, get patterns of this pyramids
            if (Top4CellPyramid != null)
            {
                res = res && Top4CellPyramid.IsReducingPossible();
                res = res && BottomLeft4CellPyramid.IsReducingPossible();
                res = res && BottomUpside4CellPyramid.IsReducingPossible();
                res = res && BottomRight4CellPyramid.IsReducingPossible();
            }
            else
            {
                res = ((TopValue == BottomLeftValue) && (TopValue == BottomUpsideValue) && (TopValue == BottomRightValue));
            }
            return res;
        }


        // Getting pattern of pyramid
        // true - values of all cells are "1"
        // false - values of all cells are "0"
        public bool Reduce(bool areducePossible)
        {
            bool res = false;
            // low level pyramids reducing is possible
            if (areducePossible)
            {
                // and level of pyramid is 1 (value containing pyramid),
                // return reduce value
                if (PyramidLevel == 1)
                    res = TopValue;
                // else, if level of pyramid is 2 (lowest group pyramid) and low level pyramids reducing is possible, 
                // reduce it
                else if (PyramidLevel == 2)
                {
                    TopValue = Top4CellPyramid.Reduce(areducePossible);
                    BottomLeftValue = BottomLeft4CellPyramid.Reduce(areducePossible);
                    BottomUpsideValue = BottomUpside4CellPyramid.Reduce(areducePossible);
                    BottomRightValue = BottomRight4CellPyramid.Reduce(areducePossible);
                    Top4CellPyramid = null;
                    BottomLeft4CellPyramid = null;
                    BottomUpside4CellPyramid = null;
                    BottomRight4CellPyramid = null;
                    res = true;
                }
                else if (PyramidLevel > 2)
                {
                    Top4CellPyramid.Reduce(areducePossible);
                    BottomLeft4CellPyramid.Reduce(areducePossible);
                    BottomUpside4CellPyramid.Reduce(areducePossible);
                    BottomRight4CellPyramid.Reduce(areducePossible);
                    res = true;
                }
                PyramidLevel--;
            }
            return res;
        }
    }
}
