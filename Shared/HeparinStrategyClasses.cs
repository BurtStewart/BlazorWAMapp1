using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeparinProtocol
{
    // so implement 6 new interfaces: 3 (Full, Cardiac, Stroke) that implement IrxIndication and 2 (aPTT, Anti-Xa ) that implement IadjPerLab
    // IrxIndication: MyFullInitialCalculator, MyCardiacInitialCalculator, MyStrokeInitialCalculator
    // IadjPerLab: MyFullaPTTAdjCalculator, MyFullAnti-XaAdjCalculator

    // 5/29/21
    // re write !   Made abstract Protocol with numerous NON changing members, then 4 concrete protocol classes, one for each type
    // (Full, Cardiac,EKOS, Stroke) which will handle either Monitoring Lab (aPTT or Xa)
    // next encapsulated the Display as it changes for each protocol, first as an Interface (IDisplatTxt) then 8 concrete classes: 2 each (aPTT and Xa) for each
    // of the 4 Indications;  the Abstract Protocol is composed (Has A) Display Interface member, which is instantiated from a Display Factory
    // (absrtact IdisplayFactory -> concrete ConcreteDisplayFactory) that has 8 Creates() to make each NEW concrete Display class

    // 6/11/21
    // note the "client is RPhHeparin which has Abstraction -> Concrete (so coding to an abstraction not an implemnetation)
    // constructor sets Indication, Lab, and PtData, the client is composed (Has A) of Display member and Protocol member,it has
    // a method SetProtocol that created a Protocol Factory and sets the Protocol per Indication, but the Display is set by the
    // Protocol Obj which maybe I should change to have a SetDisplay();

    public class RPhHeparin : IRPhHeparin  // the Client
    {
        public string Indication = string.Empty;
        public string LabToMonitor = string.Empty;
        public PtSpecificData MyPtSpecificData = new PtSpecificData();
                
        public HeparinProtocolAbstract MyHepProtocol;
        public IDisplayTxt MyDisplay;

        public void SetProtocol()
        {
            IHepProtocolFactory hepProtocolFactory = new ConcreteHepProtocolFactory(MyPtSpecificData, LabToMonitor);

            if (Indication.Equals("Full"))
                MyHepProtocol = hepProtocolFactory.CreateFullProtocol();
            else if (Indication.Equals("Cardiac"))
                MyHepProtocol = hepProtocolFactory.CreateCardiacProtocol();
            else if (Indication.Equals("EKOS"))
                MyHepProtocol = hepProtocolFactory.CreateEKOSProtocol();
            else if (Indication.Equals("Stroke"))
                MyHepProtocol = hepProtocolFactory.CreateStrokeProtocol();

        }

        public void SetDisplay()
        {            
            IdisplayFactory DisplayFactory = new ConcreteDisplayFactory();

            if (Indication.Equals("Full"))
            {
                if (LabToMonitor.Equals("aPTT"))
                    MyDisplay = DisplayFactory.CreateFullDisplay_aPTT();
                else if (LabToMonitor.Equals("Xa"))
                    MyDisplay = DisplayFactory.CreateFullDisplay_Xa();
            }
            else if (Indication.Equals("Cardiac"))
            {
                if (LabToMonitor.Equals("aPTT"))
                    MyDisplay = DisplayFactory.CreateCardiac_aPTT();
                else if (LabToMonitor.Equals("Xa"))
                    MyDisplay = DisplayFactory.CreateCardiac_Xa();
            }
            else if (Indication.Equals("EKOS"))
            {
                if (LabToMonitor.Equals("aPTT"))
                    MyDisplay = DisplayFactory.CreateEkos_aPTTdisplay();
                else if (LabToMonitor.Equals("Xa"))
                    MyDisplay = DisplayFactory.CreateEkos_Xadisplay();
            }
            else if (Indication.Equals("Stroke"))
            {
                if (LabToMonitor.Equals("aPTT"))
                    MyDisplay = DisplayFactory.CreateStroke_aPTTdisplay();
                else if (LabToMonitor.Equals("Xa"))
                    MyDisplay = DisplayFactory.CreateStroke_XAdisplay();
            }

            MyHepProtocol.DisplayProtocolTxt = MyDisplay.createDisplayTxt(MyPtSpecificData.iHt, MyPtSpecificData.dWtKg, MyHepProtocol.DosingWtKg, MyPtSpecificData.sSex,
                MyHepProtocol.BMI, MyPtSpecificData.dINR, MyPtSpecificData.dAntiXa, MyPtSpecificData.bIsPriorAnticoag,
                MyHepProtocol.InitialBolus, MyHepProtocol.InitialRate, MyHepProtocol.AdjBolusList, MyHepProtocol.AdjRateList);
        }
        
        // constructor
        public RPhHeparin(string _Indication, string _LabToMonitor,PtSpecificData _PtSpecificData)
        {
            this.Indication = _Indication;
            this.LabToMonitor = _LabToMonitor;
            this.MyPtSpecificData = _PtSpecificData;
                      
            SetProtocol();
            SetDisplay();
        }

        public RPhHeparin()
        { }
    }
    public class PtSpecificData
    {
        public int iHt;
        public double dWtKg;
        public string sSex = string.Empty;
        public double dINR;
        public double dAntiXa;
        public bool bIsPriorAnticoag;
    }
    public class ConcreteHepProtocolFactory : IHepProtocolFactory
    {
        PtSpecificData MyPtData;
        string lab;
      //  IDisplayTxt MyDisplayTxt;
      //  IdisplayFactory MyDisplayFactory = new ConcreteDisplayFactory();

        public HeparinProtocolAbstract CreateFullProtocol()
        {
            return new FullHeparinProtocol( MyPtData,lab);
        }

        public HeparinProtocolAbstract CreateCardiacProtocol()
        {
            return new CardiacACS_MI_Protocol( MyPtData, lab);
        }

        public HeparinProtocolAbstract CreateEKOSProtocol()
        {
            return new CardiacPostLyticOrEKOSProtocol( MyPtData, lab);
        }

        public HeparinProtocolAbstract CreateStrokeProtocol()
        {
            return new StrokeProtocol( MyPtData, lab);
        }

        public ConcreteHepProtocolFactory(PtSpecificData _MyPtData, string _LabToMonitor) 
        {
            this.MyPtData = _MyPtData;
            this.lab = _LabToMonitor;
          //  this.MyDisplayTxt = _DisplayTxt;
        }
    }
    public class ConcreteDisplayFactory : IdisplayFactory
    {
        public IDisplayTxt CreateFullDisplay_aPTT()
        {
            return new Full_aPTTdisplay();
        }

        public IDisplayTxt CreateFullDisplay_Xa()
        {
            return new Full_Xadisplay();
        }

        public IDisplayTxt CreateCardiac_aPTT()
        {
            return new CardiacACS_MI_aPTTdisplay();
        }

        public IDisplayTxt CreateCardiac_Xa()
        {
            return new CardiacACS_MI_Xadisplay();
        }

        public IDisplayTxt CreateEkos_aPTTdisplay()
        {
            return new CardiacPostLyticOrEKOS_aPTTdisplay();
        }

        public IDisplayTxt CreateEkos_Xadisplay()
        {
            return new CardiacPostLyticOrEKOS_Xadisplay();
        }

        public IDisplayTxt CreateStroke_aPTTdisplay()
        {
            return new StrokeDisplay_aPTT();
        }

        public IDisplayTxt CreateStroke_XAdisplay()
        {
            return new StrokeDisplay_Xa();
        }
    }
    // ==============================================
    public class FullHeparinProtocol : HeparinProtocolAbstract
    {        
        // set inputs and then call this in constructor so that outputs are available after instantiate for Display()
        public override void SetInPuts()
        {
            if (this.LabToMonitor.Equals("aPTT"))
            {

                this.InitalBolusUnitPerKg_Lab = 75;
                this.InitalRateUnitPerKgPerHr_Lab = 13;
                this.MaxInitialBolus_Lab = 10000;
                this.MaxInitialRate_Lab = 2000;

                this.BolusAdjList_Lab.Add(75);
                this.BolusAdjList_Lab.Add(40);

                this.MaxAdjBolusList_Lab.Add(10000);
                this.MaxAdjBolusList_Lab.Add(5000);

                double[] UnitPerKgAdj = { 4, 2, 1, 0, 1, 2, 3, 4 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

              //  this.displayTxt = this.DisplayFactory.CreateFullDisplay_aPTT();
            }
            else if (this.LabToMonitor.Equals("Xa"))
            {
                this.InitalBolusUnitPerKg_Lab = 75;
                this.InitalRateUnitPerKgPerHr_Lab = 13;
                this.MaxInitialBolus_Lab = 10000;
                this.MaxInitialRate_Lab = 2000;

                this.BolusAdjList_Lab.Add(60);
                this.BolusAdjList_Lab.Add(40);
                this.BolusAdjList_Lab.Add(40);

                this.MaxAdjBolusList_Lab.Add(5000);
                this.MaxAdjBolusList_Lab.Add(5000);
                this.MaxAdjBolusList_Lab.Add(5000);

                double[] UnitPerKgAdj = { 4, 3, 2, 0, 1, 2, 2, 3 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

                this.displayTxt = this.DisplayFactory.CreateFullDisplay_Xa();
            }
        }
               
        public FullHeparinProtocol( PtSpecificData _MyPtData, string _LabToMonitor) : base( _MyPtData, _LabToMonitor)
        {
            SetInPuts();
            // calc 0utputs
            this.InitialBolus = CalcInitBolus();
            this.InitialRate = CalcInitRate();
            this.AdjBolusList = CalcAdjBolusList();
            this.AdjRateList = CalcAdjRateList();
           // this.DisplayProtocolTxt = 
        }

    }
    public class Full_aPTTdisplay : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                      int InitialBolus_aPTT, int InitialRate_aPTT, List<int> AdjBolus_aPTT, List<int> AdjRate_aPTT)
        {


            List<string> resultList = new List<string>();

            resultList.Add("\t\tSTANDARD HEPARIN PROTOCOL (FULL: PE/DVT/MECHANICAL HEART VALVE/ATRIAL FIBRILATION)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.7 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus_aPTT.Equals(10000))
                resultList.Add("   Max Bolus is 10,000 units, ");
            if (InitialRate_aPTT.Equals(2000))
                resultList.Add("   Max Rate is 2000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");


            resultList.Add("    Inital Bolus dosage rounded to nearest 100" + " Units.Start rate rounded to nearest 50" + " Units/hr\r\n\r\n");
            resultList.Add("    For" + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus_aPTT.ToString("##,###") + " Units   Start at " + InitialRate_aPTT.ToString("#,###") + " Units/Hr.\r\n\r\n");
            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 10,000 units for PTT < 35 & 5,000 for PTT 35 - 48.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            resultList.Add("\tPTT < 35 sec       Bolus: " + AdjBolus_aPTT[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_aPTT[0].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 35 - 48 sec   Bolus: " + AdjBolus_aPTT[1].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_aPTT[1].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 49 - 53 sec   \t\t\t\t" + "Increase Rate By " + AdjRate_aPTT[2].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 54 - 89 sec   \t" + "   NO CHANGE\r\n");
            resultList.Add("\tPTT 90 - 95 sec   \t\t\t\t" + "Reduce Rate By " + AdjRate_aPTT[4].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 96 - 110 sec   \t\t\t\t" + "Reduce Rate By " + AdjRate_aPTT[5].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 111 - 135 sec\t   Hold drip x 1 Hr.   \t\t" + "Reduce Rate By " + AdjRate_aPTT[6].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT > 135 sec\t   Hold drip x 2 Hr.   \t\t" + "Reduce Rate By " + AdjRate_aPTT[7].ToString() + " Units/Hr\r\n");

            return resultList;
        }
    }
    public class Full_Xadisplay : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus_Xa, int InitialRate_Xa, List<int> AdjBolus_Xa, List<int> AdjRate_Xa)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\t\t\t\tSTANDARD HEPARIN PROTOCOL (FULL: PE/DVT/MECHANICAL HEART VALVE/ATRIAL FIBRILATION)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.7 DO NOT START Rx,  ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus_Xa.Equals(10000))
                resultList.Add("   Max Bolus is 10,000 units, ");
            if (InitialRate_Xa.Equals(2000))
                resultList.Add("   Max Rate is 2000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("\r\n   Inital Bolus dosage rounded to nearest 100 Units. Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For " + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus_Xa.ToString("##,###") + " Units   Start at " + InitialRate_Xa.ToString("#,###") + " Units/Hr.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            if (AntiXa >= 0.3)
            {
                resultList.Add("    Anti-Xa is " + AntiXa.ToString() + " Must us aPTT Protocol with No Bolus");
                return resultList;
            }

            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 5,000 units\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH < 0.1 IU/ml       Bolus: " + AdjBolus_Xa[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_Xa[0].ToString() + " Units/Hr\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH 0.1 - 0.24         Bolus: " + AdjBolus_Xa[1].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_Xa[1].ToString() + " Units/Hr\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH 0.25 - 0.29       Bolus: " + AdjBolus_Xa[2].ToString("#,000") + " units IV push" + "\tIncrease Rate By " + AdjRate_Xa[2].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.3 - 0.7    \t" + "   NO CHANGE\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.71 - 0.75    \t\t\t\t" + "Reduce Rate By " + AdjRate_Xa[4].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.76 - 0.8\t   Hold drip x 30 min. " + "\tReduce Rate By " + AdjRate_Xa[5].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.81 - 0.9\t   Hold drip x 60 min.   \t" + "Reduce Rate By " + AdjRate_Xa[6].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.91 - 1.0\t   Hold drip x 60 min.   \t" + "Reduce Rate By " + AdjRate_Xa[7].ToString() + " Units/Hr\r\n\r\n");

            return resultList;

        }

    }

    // =======================================================
    public class CardiacACS_MI_Protocol : HeparinProtocolAbstract
    {
        public override void SetInPuts()
        {
            if (this.LabToMonitor.Equals("aPTT"))
            {

                this.InitalBolusUnitPerKg_Lab = 60;
                this.InitalRateUnitPerKgPerHr_Lab = 12;
                this.MaxInitialBolus_Lab = 5000;
                this.MaxInitialRate_Lab = 1000;

                this.BolusAdjList_Lab.Add(60);
                this.BolusAdjList_Lab.Add(30);

                this.MaxAdjBolusList_Lab.Add(4000);
                this.MaxAdjBolusList_Lab.Add(4000);

                double[] UnitPerKgAdj = { 4, 2, 1, 0, 1, 2, 3, 4 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

                this.displayTxt = this.DisplayFactory.CreateCardiac_aPTT();
            }
            else if (this.LabToMonitor.Equals("Xa"))
            {
                this.InitalBolusUnitPerKg_Lab = 60;
                this.InitalRateUnitPerKgPerHr_Lab = 12;
                this.MaxInitialBolus_Lab = 5000;
                this.MaxInitialRate_Lab = 1000;

                this.BolusAdjList_Lab.Add(40);

                this.MaxAdjBolusList_Lab.Add(5000);

                double[] UnitPerKgAdj = { 3, 2, 1, 0, 1, 2, 2, 3 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

                this.displayTxt = this.DisplayFactory.CreateCardiac_Xa();
            }
        }
               
        public CardiacACS_MI_Protocol( PtSpecificData _MyPtData, string _LabToMonitor) : base( _MyPtData, _LabToMonitor)
        {
            SetInPuts();
            // calc 0utputs
            this.InitialBolus = CalcInitBolus();
            this.InitialRate = CalcInitRate();
            this.AdjBolusList = CalcAdjBolusList();
            this.AdjRateList = CalcAdjRateList();
          //  this.DisplayProtocolTxt = createDisplayTxt();
        }

    }
    public class CardiacACS_MI_aPTTdisplay : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus_aPTT, int InitialRate_aPTT, List<int> AdjBolus_aPTT, List<int> AdjRate_aPTT)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\t\tSTANDARD HEPARIN PROTOCOL (CARDIAC: ACS / MI)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.5 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus_aPTT.Equals(5000))
                resultList.Add("   Max Bolus is 5,000 units, ");
            if (InitialRate_aPTT.Equals(1000))
                resultList.Add("   Max Rate is 1,000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("    Inital Bolus dosage rounded to nearest 100 Units.Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For" + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus_aPTT.ToString("##,###") + " Units   Start at " + InitialRate_aPTT.ToString("#,###") + " Units/Hr.\r\n\r\n");
            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Adj Bolus: 4,000 units.\r\n\r\n"); // for PTT < 35 & 4,000 for PTT 35 - 48

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            resultList.Add("\tPTT < 35 sec       Bolus: " + AdjBolus_aPTT[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_aPTT[0].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 35 - 48 sec   Bolus: " + AdjBolus_aPTT[1].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_aPTT[1].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 49 - 53 sec   \t\t\t\t" + "Increase Rate By " + AdjRate_aPTT[2].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 54 - 89 sec   \t" + "   NO CHANGE\r\n");
            resultList.Add("\tPTT 90 - 95 sec   \t\t\t\t" + "Reduce Rate By " + AdjRate_aPTT[4].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 96 - 110 sec   \t\t\t\t" + "Reduce Rate By " + AdjRate_aPTT[5].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 111 - 135 sec\t   Hold drip x 1 Hr.   \t\t" + "Reduce Rate By " + AdjRate_aPTT[6].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT > 135 sec\t   Hold drip x 2 Hr.   \t\t" + "Reduce Rate By " + AdjRate_aPTT[7].ToString() + " Units/Hr\r\n");

            return resultList;
        }

    }
    public class CardiacACS_MI_Xadisplay : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus_Xa, int InitialRate_Xa, List<int> AdjBolus_Xa, List<int> AdjRate_Xa)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\t\t\t\tSTANDARD HEPARIN PROTOCOL (ACS / MI)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.7 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus_Xa.Equals(5000))
                resultList.Add("   Max Bolus is 5,000 units, ");
            if (InitialRate_Xa.Equals(1000))
                resultList.Add("   Max Rate is 1000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("\r\n   Inital Bolus dosage rounded to nearest 100 Units. Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For " + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus_Xa.ToString("##,###") + " Units   Start at " + InitialRate_Xa.ToString("#,###") + " Units/Hr.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            if (AntiXa >= 0.3)
            {
                resultList.Add("    Anti-Xa is " + AntiXa.ToString() + " Must use aPTT Protocol with No Bolus");
                return resultList;
            }

            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 5,000 units\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH < 0.1 IU/ml       Bolus: " + AdjBolus_Xa[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRate_Xa[0].ToString() + " Units/Hr\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH 0.1 - 0.19     \t\t\t\tIncrease Rate By " + AdjRate_Xa[1].ToString() + " Units/Hr\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH 0.2 - 0.29      \t\t\t\tIncrease Rate By " + AdjRate_Xa[2].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.3 - 0.5    \t" + "   NO CHANGE\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.51 - 0.6    \t\t\t\t" + "Reduce Rate By " + AdjRate_Xa[4].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.61 - 0.7\t   Hold drip x 30 min. " + "\tReduce Rate By " + AdjRate_Xa[5].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.71 - 0.8\t   Hold drip x 60 min.   \t" + "Reduce Rate By " + AdjRate_Xa[6].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.81 - 1.0\t   Hold drip x 90 min.   \t" + "Reduce Rate By " + AdjRate_Xa[7].ToString() + " Units/Hr\r\n\r\n");

            return resultList;
        }

    }

    // ======================================================
    public class CardiacPostLyticOrEKOSProtocol : HeparinProtocolAbstract
    {
        public override void SetInPuts()
        {
            if (this.LabToMonitor.Equals("aPTT"))
            {

                this.InitalBolusUnitPerKg_Lab = 60;
                this.InitalRateUnitPerKgPerHr_Lab = 12;
                this.MaxInitialBolus_Lab = 4000;
                this.MaxInitialRate_Lab = 1000;

                this.BolusAdjList_Lab.Add(60);
                this.BolusAdjList_Lab.Add(30);

                this.MaxAdjBolusList_Lab.Add(4000);
                this.MaxAdjBolusList_Lab.Add(4000);

                double[] UnitPerKgAdj = { 4, 2, 1, 0, 1, 2, 3, 4 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

                this.displayTxt = this.DisplayFactory.CreateEkos_aPTTdisplay();
            }
            else if (this.LabToMonitor.Equals("Xa"))
            {
                this.InitalBolusUnitPerKg_Lab = 60;
                this.InitalRateUnitPerKgPerHr_Lab = 12;
                this.MaxInitialBolus_Lab = 4000;
                this.MaxInitialRate_Lab = 1000;

                this.BolusAdjList_Lab.Add(60);
                this.BolusAdjList_Lab.Add(30);

                this.MaxAdjBolusList_Lab.Add(4000);
                this.MaxAdjBolusList_Lab.Add(4000);

                double[] UnitPerKgAdj = { 3, 2, 1, 0, 1, 2, 2, 3 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

                this.displayTxt = this.DisplayFactory.CreateEkos_Xadisplay();
            }
        }

        public CardiacPostLyticOrEKOSProtocol( PtSpecificData _MyPtData, string _LabToMonitor) : base( _MyPtData, _LabToMonitor)
        {
            SetInPuts();

            this.InitialBolus = CalcInitBolus();
            this.InitialRate = CalcInitRate();
            this.AdjBolusList = CalcAdjBolusList();
            this.AdjRateList = CalcAdjRateList();
        //    this.DisplayProtocolTxt = createDisplayTxt();
        }
    }
    public class CardiacPostLyticOrEKOS_aPTTdisplay : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus, int InitialRate, List<int> AdjBolusList, List<int> AdjRateList)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\t\tSTANDARD HEPARIN PROTOCOL (EKOS, Post-Thrombolytic PE)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.5 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus.Equals(4000))
                resultList.Add("   Max Bolus is 4,000 units, ");
            if (InitialRate.Equals(1000))
                resultList.Add("   Max Rate is 1,000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("    Inital Bolus dosage rounded to nearest 100 Units.Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For" + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus.ToString("##,###") + " Units   Start at " + InitialRate.ToString("#,###") + " Units/Hr.\r\n\r\n");
            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 4,000 units for PTT < 35 & 4,000 for PTT 35 - 48.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            resultList.Add("\tPTT < 35 sec       Bolus: " + AdjBolusList[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRateList[0].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 35 - 48 sec   Bolus: " + AdjBolusList[1].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRateList[1].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 49 - 53 sec   \t\t\t\t" + "Increase Rate By " + AdjRateList[2].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 54 - 89 sec   \t" + "   NO CHANGE\r\n");
            resultList.Add("\tPTT 90 - 95 sec   \t\t\t\t" + "Reduce Rate By " + AdjRateList[4].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 96 - 110 sec   \t\t\t\t" + "Reduce Rate By " + AdjRateList[5].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 111 - 135 sec\t   Hold drip x 1 Hr.   \t\t" + "Reduce Rate By " + AdjRateList[6].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT > 135 sec\t   Hold drip x 2 Hr.   \t\t" + "Reduce Rate By " + AdjRateList[7].ToString() + " Units/Hr\r\n");

            return resultList;
        }

    }
    public class CardiacPostLyticOrEKOS_Xadisplay : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus, int InitialRate, List<int> AdjBolusList, List<int> AdjRateList)
        {
            List<string> resultList = new List<string>();

            resultList.Add("   CAUTION: THERE IS NO OFFICAL PROTOCOL for this USING Anti-Xa. This is only what Burt MIGHT use !!!\r\n\r\n");
            resultList.Add("\t\t\t\tUNOFFICIAL (EKOS, Post-Thrombolytic PE)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.5 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus.Equals(10000))
                resultList.Add("   Max Bolus is 4,000 units, ");
            if (InitialRate.Equals(1000))
                resultList.Add("   Max Rate is 1000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("\r\n   Inital Bolus dosage rounded to nearest 100 Units. Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For " + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus.ToString("##,###") + " Units   Start at " + InitialRate.ToString("#,###") + " Units/Hr.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to neares 50 Units/hr\r\n\r\n");

            if (AntiXa >= 0.3)
            {
                resultList.Add("    Anti-Xa is " + AntiXa.ToString() + " Must use aPTT Protocol with No Bolus");
                return resultList;
            }

            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 5,000 units\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH < 0.1 IU/ml       Bolus: " + AdjBolusList[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRateList[0].ToString() + " Units/Hr\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH 0.1 - 0.19          Bolus: " + AdjBolusList[1].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRateList[1].ToString() + " Units/Hr\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH 0.2 - 0.29      \t\t\t\tIncrease Rate By " + AdjRateList[2].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.3 - 0.5    \t" + "   NO CHANGE\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.51 - 0.6    \t\t\t\t" + "Reduce Rate By " + AdjRateList[4].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.61 - 0.7\t   Hold drip x 30 min. " + "\tReduce Rate By " + AdjRateList[5].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.71 - 0.8\t   Hold drip x 60 min.   \t" + "Reduce Rate By " + AdjRateList[6].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.81 - 1.0\t   Hold drip x 90 min.   \t" + "Reduce Rate By " + AdjRateList[7].ToString() + " Units/Hr\r\n\r\n");


            return resultList;
        }

    }

    // =======================================================
    public class StrokeProtocol : HeparinProtocolAbstract
    {
        public override void SetInPuts()
        {
            if (this.LabToMonitor.Equals("aPTT"))
            {

                this.InitalBolusUnitPerKg_Lab = 0;   // no bolus
                this.InitalRateUnitPerKgPerHr_Lab = 12;
                this.MaxInitialBolus_Lab = 0;
                this.MaxInitialRate_Lab = 1000;

                this.BolusAdjList_Lab.Add(50);
                this.BolusAdjList_Lab.Add(25);

                this.MaxAdjBolusList_Lab.Add(3000);
                this.MaxAdjBolusList_Lab.Add(2000);

                double[] UnitPerKgAdj = { 2, 1, 0.5, 0, 0.5, 1, 1.5, 2, 2.5, 3 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

                this.displayTxt = this.DisplayFactory.CreateStroke_aPTTdisplay();
            }
            else if (this.LabToMonitor.Equals("Xa"))
            {
                this.InitalBolusUnitPerKg_Lab = 0;
                this.InitalRateUnitPerKgPerHr_Lab = 12;
                this.MaxInitialBolus_Lab = 0;
                this.MaxInitialRate_Lab = 1000;

                this.BolusAdjList_Lab.Add(40);

                this.MaxAdjBolusList_Lab.Add(5000);

                double[] UnitPerKgAdj = { 3, 2, 1, 0, 1, 2, 2, 3 };
                this.RateAdjList_Lab.AddRange(UnitPerKgAdj);

               this.displayTxt = this.DisplayFactory.CreateStroke_XAdisplay();
            }
        }

        public StrokeProtocol( PtSpecificData _MyPtData, string _LabToMonitor) : base( _MyPtData, _LabToMonitor)
        {
            SetInPuts();

            this.InitialBolus = CalcInitBolus();
            this.InitialRate = CalcInitRate();
            this.AdjBolusList = CalcAdjBolusList();
            this.AdjRateList = CalcAdjRateList();
          //  this.DisplayProtocolTxt = createDisplayTxt();
        }
    }
    public class StrokeDisplay_aPTT : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus, int InitialRate, List<int> AdjBolusList, List<int> AdjRateList)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\t\tSTANDARD HEPARIN PROTOCOL (Acute Ischemic Stroke/Cerebral Venous Sinus Thrombosis/Arterial Dissection)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.5 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus.Equals(4000))
                resultList.Add("   Max Bolus is 0 units, ");
            if (InitialRate.Equals(1000))
                resultList.Add("   Max Rate is 1,000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("    Inital Bolus dosage rounded to nearest 100 Units.Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For" + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus.ToString("##,###") + " Units   Start at " + InitialRate.ToString("#,###") + " Units/Hr.\r\n\r\n");
            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 4,000 units for PTT < 35 & 4,000 for PTT 35 - 48.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            resultList.Add("\tPTT < 35 sec       Bolus: " + AdjBolusList[0].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRateList[0].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 35 - 45 sec   Bolus: " + AdjBolusList[1].ToString("#,000") + " units IV push\t" + "Increase Rate By " + AdjRateList[1].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 46 - 49 sec   \t\t\t\t" + "Increase Rate By " + AdjRateList[2].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 50 - 65 sec   \t" + "   NO CHANGE\r\n");
            resultList.Add("\tPTT 66 - 70 sec   \t\t\t\t" + "Reduce Rate By " + AdjRateList[4].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 71 - 80 sec   \t\t\t\t" + "Reduce Rate By " + AdjRateList[5].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 81 - 90 sec\t   Hold drip x 1 Hr.   \t\t" + "Reduce Rate By " + AdjRateList[6].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 91 - 100 sec\t   Hold drip x 1 Hr.   \t\t" + "Reduce Rate By " + AdjRateList[7].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT 101 - 120 sec\t   Hold drip x 2 Hr.   \t\t" + "Reduce Rate By " + AdjRateList[8].ToString() + " Units/Hr\r\n");
            resultList.Add("\tPTT > 120 sec\t   Hold drip x 2.5 Hr.   \t" + "Reduce Rate By " + AdjRateList[9].ToString() + " Units/Hr\r\n");


            return resultList;
        }
    }
    public class StrokeDisplay_Xa : IDisplayTxt
    {
        public List<string> createDisplayTxt(int Ht, double WtKg, double DosingWtKg, string Sex, double BMI, double INR, double AntiXa, bool IsPriorAnticoag,
                                     int InitialBolus, int InitialRate, List<int> AdjBolusList, List<int> AdjRateList)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\t\t\t\tSTANDARD HEPARIN PROTOCOL ACUTE ISCHEMIC STROKE (Cerebral venous sinus thrombosis, Arterial Dissection)\r\n\r\n");
            if (INR > 2.0)
                resultList.Add("   INR > 2.0, No Bolus, ");
            if (AntiXa >= 0.3)
                resultList.Add("   Anti-Xa >= 0.3, No Bolus and must use aPTT, NOTE IF > 0.5 DO NOT START Rx, ");
            if (INR > 1.5)
                resultList.Add("   INR > 1.5 so Start Rate is 10 units/kg/hr, ");
            if (InitialBolus.Equals(10000))
                resultList.Add("   Max Bolus is 0 units, ");
            if (InitialRate.Equals(1000))
                resultList.Add("   Max Rate is 1000 units/hr, ");
            if (IsPriorAnticoag)
                resultList.Add("   Caution!, Prior Anticoag may alter protocol, see Protocol, ");

            resultList.Add("\r\n   Inital Bolus dosage rounded to nearest 100 Units. Start rate rounded to nearest 50 Units/hr\r\n\r\n");
            resultList.Add("    For " + Ht.ToString() + " In " + WtKg.ToString() + " Kg  " + Sex + " : BMI = " + BMI.ToString("##.#") + "  INR = " + INR.ToString() + " Anti-Xa = " + AntiXa.ToString() + "\r\n\r\n");
            resultList.Add("    Inital Bolus = " + InitialBolus.ToString("##,###") + " Units   Start at " + InitialRate.ToString("#,###") + " Units/Hr.\r\n\r\n");

            resultList.Add("    Adjustment Bolus dosage rounded to nearest 100 Units. Adjustment rate rounded to nearest 50 Units/hr\r\n\r\n");

            if (AntiXa >= 0.3)
            {
                resultList.Add("    Anti-Xa is " + AntiXa.ToString() + " Must use aPTT Protocol with No Bolus");
                return resultList;
            }

            if (BMI > 30)
                resultList.Add("   BMI > 30 using AdjWt of  " + DosingWtKg.ToString() + " Kg,  Max Bolus: 5,000 units\r\n\r\n");

            resultList.Add("\tAnti-Xa UFH < 0.1 IU/ml    \t\t\t\tIncrease Rate By " + AdjRateList[0].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.1 - 0.19     \t\t\t\tIncrease Rate By " + AdjRateList[1].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.2 - 0.29      \t\t\t\tIncrease Rate By " + AdjRateList[2].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.3 - 0.5    \t" + "   NO CHANGE\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.51 - 0.6    \t\t\t\t" + "Reduce Rate By " + AdjRateList[4].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.61 - 0.7\t   Hold drip x 30 min. " + "\tReduce Rate By " + AdjRateList[5].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.71 - 0.8\t   Hold drip x 60 min.   \t" + "Reduce Rate By " + AdjRateList[6].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH 0.81 - 1.0\t   Hold drip x 90 min.   \t" + "Reduce Rate By " + AdjRateList[7].ToString() + " Units/Hr\r\n\r\n");
            resultList.Add("\tAnti-Xa UFH > 1.0\t  See Table on Management of Anti-Xa level > 1");


            return resultList;
        }

    }


}

