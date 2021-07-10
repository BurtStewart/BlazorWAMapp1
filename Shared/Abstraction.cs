using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeparinProtocol
{
    // STRATEGY PATTERN per "Head First Design Patterns - 10th Anniversary"
    // The Strategy Pattern defines a family of algorithms, encapsulates each one, and makes them interchangeable.
    // Strategy lets the algorithm vary independently from clients that use it.

    // OO Basics: Abstraction, Encapsulation, Polymorphism, Inheritance
    // OO Principles: 
    // Encapsulate what varies (changes).  and separate them from what stays the same
    // Favor composition over inheritance.
    // Program to interfaces (abstract class/Interface class), not implementations.  ie design to abstract classes not concrete implementation classes

    //  things I think will change, bolus and rates are func of Indication and which Lab (aPTT or anti-Xa) detemines dosage adjustments
    // so make as Interface: Indication and Dose Adjustment per Lab

    public interface IRPhHeparin
    {
         void SetProtocol();
         void SetDisplay();
    }

    public interface IHepProtocolFactory
    {
        HeparinProtocolAbstract CreateFullProtocol();
        HeparinProtocolAbstract CreateCardiacProtocol();
        HeparinProtocolAbstract CreateEKOSProtocol();
        HeparinProtocolAbstract CreateStrokeProtocol();
    }
    public interface IDisplayTxt
    {
        List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                      int InitialBolus_Lab, int InitialRate_Lab, List<int> AdjBolus_Lab, List<int> AdjRate_Lab);
    }

    public interface IdisplayFactory
    {
        IDisplayTxt CreateFullDisplay_aPTT();
        IDisplayTxt CreateFullDisplay_Xa();
        IDisplayTxt CreateCardiac_aPTT();
        IDisplayTxt CreateCardiac_Xa();
        IDisplayTxt CreateEkos_aPTTdisplay();
        IDisplayTxt CreateEkos_Xadisplay();
        IDisplayTxt CreateStroke_aPTTdisplay();
        IDisplayTxt CreateStroke_XAdisplay();

    }
       // so need 2 more for each indication    so maybe I don't need Indication as instance variables in Protocol?
    public abstract class HeparinProtocolAbstract
    {
        // inputs
        //  public string Indication;
        public string Indication = string.Empty;
        public string LabToMonitor = string.Empty;
        public IdisplayFactory DisplayFactory = new ConcreteDisplayFactory();
        // do abstract createDisplayTxt() here and implement in each concrete class
     //   public abstract List<string> createDisplayTxt(); 
        public IDisplayTxt displayTxt;

        public readonly string Sex;
        public readonly int HtIn;
        public readonly double WtKg;
        public readonly double BMI;
        public double DosingWtKg;
        public readonly double INR;
        public readonly double AntiXa;
        public bool IsPriorAntiCoag;
        
        public int InitalBolusUnitPerKg_Lab;
        public int MaxInitialBolus_Lab;
        public int InitalRateUnitPerKgPerHr_Lab;
        public int MaxInitialRate_Lab;
        public List<int> BolusAdjList_Lab;
        public List<double> RateAdjList_Lab;
        public List<int> MaxAdjBolusList_Lab;
                
        // outputs
        public int InitialBolus;
        public int InitialRate;
        public List<int> AdjBolusList;
        public List<int> AdjRateList;
        public List<string> DisplayProtocolTxt = new List<string>();
               
        // Implement non
        // changing methods
        public int RoundToNearest(int Nearest, double Number)
        {
            if (Number.Equals(0)) // bug of NaN
                return 0;
            double dTemp;
            dTemp = Math.Truncate(Number / Nearest + 0.5);
            return (int)Math.Round(dTemp * Nearest);
        }

        public double CalcBMI()
        {
            // Wt(#) / Ht^2 (in) *703   nl : 18.5 - 24.9  (25 -29.9 Overweight, > 30 Obese)
            // given Ht(in) & Wt (#) return BMI, all strings, nl range is 18.5 to 24.9

            double PtBMI;
            double WtLbs = this.WtKg * 2.2;

            // floating point operations never throw an exception !
            // so if Ht is 0, result is infinity ! or divide by zero gives NaN (not a number)
            double SqrPtHtIn = Math.Pow(this.HtIn, 2);
            PtBMI = WtLbs / (SqrPtHtIn) * 703;
            if (SqrPtHtIn == 0)
                PtBMI = 0;

            return PtBMI;
        }

        public double CalcIBWKg()
        {
            // Ideal Body Weight (IBW)
            //  1.Male = 50 kg + 2.3 kg for each 1” over 60”
            //  2.Female = 45.5 kg + 2.3 kg for each 1” over 60”.
            double result = 0.0;
            double BaseWt = 0.0;
            double HtDiff = 0.0;
            // set Base Wt
            if (this.Sex.ToUpper().Equals("M"))
                BaseWt = 50;
            else
                BaseWt = 45.5;
            // calc Wt adjustment for Ht (up + or down -)
            HtDiff = this.HtIn - 60;

            result = BaseWt + (HtDiff * 2.3);

            return result;
        }

        public double CalcAdjWtKg()
        {
            // Heparin dosing in obese patients: if patient's BMI > 30:
            // 1.Use actual(or Total) Body Weight(TBW) to calculate initial heparin bolus(maximum for
            // DVT / PE / mechanical heart valve, atrial fibrillation: 10, 000 units; max. for ACS: 5, 000 units) and initial infusion
            // rate(maximum for DVT / PE / mechanical heart valve, atrial fibrillation: 2, 000 units / hr; max. for ACS: 1000
            // units / hr; max. for High bleeding risk protocol: 1200 units / hr).

            // 2. Use adjusted body weight for dose adjustment nomogram.
            // 2.Adjusted Body Weight = IBW + [0.4(TBW – IBW)]
            // 
            double result = 0.0;
            double IBWKg = this.CalcIBWKg();
            double WtKgAdj = 0.4 * (this.WtKg - IBWKg);

            result = IBWKg + WtKgAdj;

            return result;
        }

        public int CalcInitBolus()
        {
            // 1. Use actual (or Total) Body Weight (TBW) to calculate initial heparin bolus (maximum for
            //  DVT / PE / mechanical heart valve, atrial fibrillation: 10,000 units; max. for ACS: 5, 000 units) and initial infusion
            //  rate(maximum for DVT / PE / mechanical heart valve, atrial fibrillation: 2, 000 units / hr; max. for ACS: 1000
            //  units / hr; max. for High bleeding risk protocol: 1200 units / hr).

            // 75 u/Kg  Max : 10,000 units
            // (If INR>2.0: no blous)

            //    -----------------------------------------------------------------

            //    .If baseline anti-Xa level is between 0.3 - 0.7(for Full Dose protocol) or 0.3 - 0.5(for Cardiac dose
            // and Stroke dose protocol): hold heparin bolus and start heparin infusion as per protocol.
            int result = 0;
            int InitalBolusUnitPerKg = 0;
            int MaxInitialBolus = 0;

            // set local variable for each Monitored Lab
            //if (this.LabToMonitor.Equals("aPTT"))
            //{
            InitalBolusUnitPerKg = this.InitalBolusUnitPerKg_Lab;
            MaxInitialBolus = this.MaxInitialBolus_Lab;
            //}
            //else if (this.LabToMonitor.Equals("Anti-Xa"))
            //{
            //    InitalBolusUnitPerKg = this.InitalBolusUnitPerKg_Xa;
            //    MaxInitialBolus = this.MaxInitialBolus_Xa;
            //}

            if (INR <= 2.0 & AntiXa < 0.3)
            {
                double TempResult = InitalBolusUnitPerKg * WtKg;
                TempResult = Math.Round(TempResult);
                TempResult = this.RoundToNearest(100, TempResult);
                if (TempResult > MaxInitialBolus)
                    TempResult = MaxInitialBolus;  // Max: 10,000 units

                result = (int)TempResult;
            }

            return result;
        }
        public int CalcInitRate()
        {
            int result = 0;
            int InitalRateUnitPerKgPerHr = 0;
            int MaxInitialRate = 0;

            // set local variable for each Monitored Lab
            //if (this.LabToMonitor.Equals("aPTT"))
            //{
            InitalRateUnitPerKgPerHr = this.InitalRateUnitPerKgPerHr_Lab;
            MaxInitialRate = this.MaxInitialRate_Lab;
            //}
            //else if (this.LabToMonitor.Equals("Anti-Xa"))
            //{
            //    InitalRateUnitPerKgPerHr = this.InitalRateUnitPerKgPerHr_Xa;
            //    MaxInitialRate = this.MaxInitialRate_Xa;
            //}

            if (INR > 1.5)
                InitalRateUnitPerKgPerHr = 10;

            double TempResult = WtKg * InitalRateUnitPerKgPerHr;
            TempResult = Math.Round(TempResult);
            TempResult = this.RoundToNearest(50, TempResult);
            if (TempResult > MaxInitialRate)
                TempResult = MaxInitialRate;

            result = (int)TempResult;

            return result;
        }

        public List<int> CalcAdjBolusList()
        {
            List<int> result = new List<int>();

            List<int> BolusAdjList = new List<int>();
            List<int> MaxAdjBolusList = new List<int>();


            // set local variable for each Monitored Lab
            //if (this.LabToMonitor.Equals("aPTT"))
            //{
            BolusAdjList = this.BolusAdjList_Lab;
            MaxAdjBolusList = this.MaxAdjBolusList_Lab;
            //}
            //else if (this.LabToMonitor.Equals("Anti-Xa"))
            //{
            //    BolusAdjList = this.BolusAdjList_Xa;
            //    MaxAdjBolusList = this.MaxAdjBolusList_Xa;
            //}

            //     int[] BolusAdjUnitPerKgArray = { 75, 40 };
            //     int[] MaxBolusArray = { 10000, 5000 };

            double TempResult = 0;

            for (int i = 0; i < BolusAdjList.Count; i++)
            {
                TempResult = BolusAdjList[i] * DosingWtKg;
                TempResult = Math.Round(TempResult);
                TempResult = this.RoundToNearest(100, TempResult);
                if (TempResult > MaxAdjBolusList[i])
                    TempResult = MaxAdjBolusList[i];  // Max: 10,000 units

                result.Add((int)TempResult);
            }

            return result;
        }

        public List<int> CalcAdjRateList()
        {
            List<int> result = new List<int>();

            List<double> UnitPerKgAdj = new List<double>();

            // set local variable for each Monitored Lab
            //if (this.LabToMonitor.Equals("aPTT"))
            //{
            UnitPerKgAdj = RateAdjList_Lab;
            //}
            //else if (this.LabToMonitor.Equals("Anti-Xa"))
            //{
            //    UnitPerKgAdj = RateAdjList_Xa;
            //}

            //int[] resultArray = new int[8];
            //int[] UnitPerKgAdj = { 4, 2, 1, 0, 1, 2, 3, 4 };
            double TempResult = 0;

            for (int i = 0; i < UnitPerKgAdj.Count; i++)
            {
                TempResult = UnitPerKgAdj[i] * DosingWtKg;
                TempResult = Math.Round(TempResult);
                TempResult = this.RoundToNearest(50, TempResult);

                result.Add((int)TempResult);
            }

            return result;
        }
      //  public List<string> createDisplayTxt()
        //{
        //    List<string> result = new List<string>();

        //    result = this.displayTxt.createDisplayTxt(this.HtIn, WtKg, DosingWtKg, Sex, BMI, INR, AntiXa, IsPriorAntiCoag, InitialBolus, InitialRate, AdjBolusList, AdjRateList);

        //    return result;
        //}

        public abstract void SetInPuts();

        // constructor
        public HeparinProtocolAbstract(PtSpecificData _MyPtData, string _LabToMonitor)
        {
            this.LabToMonitor = _LabToMonitor;
            
            this.Sex = _MyPtData.sSex;
            this.HtIn = _MyPtData.iHt;
            this.WtKg = _MyPtData.dWtKg;
            this.BMI = CalcBMI();
            if (BMI > 30)
                this.DosingWtKg = CalcAdjWtKg();
            else
                this.DosingWtKg = WtKg;
            this.INR = _MyPtData.dINR;
            this.AntiXa = _MyPtData.dAntiXa;
            this.IsPriorAntiCoag = _MyPtData.bIsPriorAnticoag;
            // initialize lists
            // input lists
            BolusAdjList_Lab = new List<int>();
            RateAdjList_Lab = new List<double>(); 
            MaxAdjBolusList_Lab = new List<int>();

            // output lists
            this.AdjBolusList = new List<int>();
            AdjRateList = new List<int>();

            SetInPuts();
                     
        }

        protected HeparinProtocolAbstract()  // required to compile ?
        {
        }
    }

            
}
