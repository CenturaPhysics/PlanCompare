using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;

namespace PlanCompare {
    public class DataManager {

        public Course[] theCourses = new Course[3];
        public PlanSetup[] thePlans = new PlanSetup[3];


        //Class to hold data for plan comparisons.  One of these is created for each plan.  We make these so we can
        //more easily iterate through for plan comparison.  The General field info is a List of CompareListItems.
        //The Field info is a nested List of Lists of CompareListItems, one inner list for each field.
        public class PlanCompareLists {
            public List<CompareListItem> clGenInfo;
            public List<List<CompareListItem>> clFieldInfo;

            //Constructor
            public PlanCompareLists()
            {
                clGenInfo = new List<CompareListItem>();
                clFieldInfo = new List<List<CompareListItem>>();
            }
        }


        //Create a struct to hold data for comparing each field.  One of these is added to the PlanCompareListStruct for each field.
        //Data item can be of type double or string.  Convert integers to double before storing.
        public struct CompareListItem {
            public int dataType;  //use 1=double, 2=string
            public string dataTag;  //used to indentify the held data.  Currently unused, but set to a string describing the data item.
            public double numData;  //used to store numeric data.  Set to -999 if unused.
            public string stringData;  //used to store string data.  Set to "" if unused.
            public string cbName;  //the name of the checkbox used to show comparison result

            public CompareListItem(int aDataType, string aDataTag, double aNumData, string aStringData, string aCBName)
            {
                dataType = aDataType;
                dataTag = aDataTag;
                numData = aNumData;
                stringData = aStringData;
                cbName = aCBName;
            }
        }

        public class PlanCompareResults {
            public List<bool> clGenResults;
            public List<bool> clFieldResults;

            //Constructor
            public PlanCompareResults()
            {
                clGenResults = new List<bool>();
                clFieldResults = new List<bool>();
            }
        }

        public PlanCompareLists[] planCompLists = new PlanCompareLists[3];
        public PlanCompareResults planCompareResults;

        public TextBox debugTB;


        //Constructor
        public DataManager()
        {
            theCourses[0] = null;
            theCourses[1] = null;
            theCourses[2] = null;

            thePlans[0] = null;
            thePlans[1] = null;
            thePlans[2] = null;

            planCompLists[0] = null;
            planCompLists[1] = new PlanCompareLists();
            planCompLists[2] = new PlanCompareLists();

            planCompareResults = new PlanCompareResults();
        }


        //A function for presenting the plan data.  First it updates the plan info in the general section by accessing the TextBlocks
        //stored in the passed TextBlock list.  Next, it adds data to the field grid (passed as parameter) wih a column offset.
        //This function should be agnostic with respect to the interface... just updating the interface objects that are sent.
        //Last, it calls the SetCompareListForPlan function to update the lists of comparison items and results.
        //MessageBox.Show("Start of LoadDataForPlan(" + planNum.ToString() + ").");
        public void SetPlanData(int planNum, Grid aFieldGrid, List<TextBlock> genTextBlockList, List<TextBlock> fldTextBlockList, int gridColOffset) {


            if (theCourses[planNum] != null) {
                genTextBlockList[0].Text = theCourses[planNum].Id.ToString();

                if (thePlans[planNum] != null) {
                    //MessageBox.Show("Made it into the if statements of LoadDataForPlan(" + planNum.ToString() + ").");
                    PlanSetup aPlan = thePlans[planNum];

                    genTextBlockList[1].Text = aPlan.Id.ToString();
                    genTextBlockList[2].Text = aPlan.Beams.Count().ToString();
                    genTextBlockList[3].Text = aPlan.PhotonCalculationModel;

                    int fldCount = aPlan.Beams.Count();

                    for (int i = 0; i < fldCount; i++) {
                        Beam curBeam = aPlan.Beams.ElementAt(i);

                        TextBlock newTB = new TextBlock();
                        newTB.Text = curBeam.Id;
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 65, 2 + i, gridColOffset + 1);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].GantryAngle.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 2);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].CollimatorAngle.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 3);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].PatientSupportAngle.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 4);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.X1.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 5);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.X2.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 6);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.Y1.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 7);
                        fldTextBlockList.Add(newTB);

                        newTB = new TextBlock();
                        newTB.Text = curBeam.ControlPoints[0].JawPositions.Y2.ToString();
                        AddTextBlockToFieldGridAt(aFieldGrid, newTB, 50, 2 + i, gridColOffset + 8);
                        fldTextBlockList.Add(newTB);
                    }

                    SetCompareListForPlan(planNum);
                    //MessageBox.Show("Just before checking if planNum is 2.  Current planNum = " + planNum.ToString() );
                    if (planNum == 2) {
                        Check_Comparison();
                    }
                }
            }
        }


        //Utility function for above LoadDataForPlan.  Allows the adding of a TextBlock to a grid cell in one function.
        public void AddTextBlockToFieldGridAt(Grid aGrid, TextBlock aTB, int aWidth, int aRow, int aCol)
        {
            aTB.FontSize = 12;
            aTB.Width = aWidth;
            aTB.TextAlignment = TextAlignment.Center;
            aTB.Margin = new Thickness(0, 4, 0, 4);
            aGrid.Children.Add(aTB);
            Grid.SetRow(aTB, aRow);
            Grid.SetColumn(aTB, aCol);
        }


        //Function to clear all plan data.  Currently, just clears the plan comparison list, but may be modified for additional
        //data clearing later.
        public void ClearPlanData(int planNum)
        {
            ClearCompareListForPlan(planNum);
        }


        //Function for adding plan comparison info to the PlanCompareLists item specified by the plan number.  The General Info
        //is simply a list of CompareListItems.  The Field info is a nested list of lists of CompareListItems, where we add a new 
        //List of CompareListItems for each field.  Thus, to access the comparison data, we have:  planCompLists[planNum].cfields[fieldNum]
        public void SetCompareListForPlan(int planNum)
        {
            //Before continuing, ensure that the plan has been stored in the data manager array of plans.
            if( thePlans[planNum] != null ) {
                //Clear the current plan comparison data, so that we can add new fresh data.
                ClearCompareListForPlan(planNum);
                int fldCount = thePlans[planNum].Beams.Count();

                //Add an initial result to the General list.  This is item 0, and will be used to display the All-Fields result.
                planCompareResults.clGenResults.Add(true);  //item 0

                //Add the General field info.  Currently this is just one item... the number of fields.
                planCompLists[planNum].clGenInfo.Add(new CompareListItem(1, "NumOfFields", fldCount, "", "compNumOfFields"));
                planCompareResults.clGenResults.Add(true);  //item 1

                //For each field, add a list of CompareListItems.  Then, add CompareListItems to each inner list for each field parameter.
                //Also add an entry to the list of field comparison results
                for (int i = 0; i < fldCount; i++) {
                    planCompLists[planNum].clFieldInfo.Add( new List<CompareListItem>() );
                    planCompareResults.clFieldResults.Add(true);

                    Beam beam = thePlans[planNum].Beams.ElementAt(i);
                    string fldNumStr = i.ToString();
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":GantryAngle", beam.ControlPoints[0].GantryAngle, "", "compF" + i.ToString()));
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":CollAngle", beam.ControlPoints[0].CollimatorAngle, "", "compF" + i.ToString()));
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":TableAngle", beam.ControlPoints[0].PatientSupportAngle, "", "compF" + i.ToString()));
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":X1", beam.ControlPoints[0].JawPositions.X1, "", "compF" + i.ToString()));
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":X2", beam.ControlPoints[0].JawPositions.X2, "", "compF" + i.ToString()));
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":Y1", beam.ControlPoints[0].JawPositions.Y1, "", "compF" + i.ToString()));
                    planCompLists[planNum].clFieldInfo[i].Add(new CompareListItem(1, "F" + fldNumStr + ":Y2", beam.ControlPoints[0].JawPositions.Y2, "", "compF" + i.ToString()));
                }
            }
        }


        //Clear the PlanCompareLists class data, and also clears the PlanCompareResults data.
        public void ClearCompareListForPlan(int planNum)
        {
            planCompLists[planNum].clGenInfo.Clear();
            foreach ( List<CompareListItem> cList in planCompLists[planNum].clFieldInfo ) { cList.Clear(); }
            planCompLists[planNum].clFieldInfo.Clear();

            planCompareResults.clGenResults.Clear();
            planCompareResults.clFieldResults.Clear();
        }


        //A function for comparing plans. Starts by comparing the general info.  Sets marker to red if doesn't match.
        //For field data, starts by assuming that the data for each field matches, and sets the field_okay marker
        //to green.  Then, itereates through and sets the field_okay marker to red if it encounters a discrepancy.
        public void Check_Comparison()
        {
            int compGenCnt = planCompLists[1].clGenInfo.Count();
            int compFldCnt = planCompLists[1].clFieldInfo.Count();

            //Double check that our results lists are the same length as the actual comparison items list.  If not, alert user and exit function.
            if ( (planCompareResults.clGenResults.Count()-1) != compGenCnt ) {
                //MessageBox.Show("Results list for general info different length than list of general info.");
                return;
            }

            if ( planCompareResults.clFieldResults.Count() != compFldCnt ) {
                //MessageBox.Show("Results list for field info different length than list of field info.");
                return;
            }

            //Iterate through the general info.  Compare either numeric or string data.  Set the results list item to the boolean of whether the
            //two data items are equal.
            //Start from item 1, as item 0 is reserved for the all-fields result, and will be updated below.
            for (int i = 1; i < compGenCnt; i++) {
                
                if (planCompLists[1].clGenInfo[i].dataType == 1) {
                    planCompareResults.clGenResults[i] = planCompLists[1].clGenInfo[i].numData == planCompLists[2].clGenInfo[i].numData;
                    debugTB.AppendText("Plan1 clGenInfo[" + i.ToString() + "] = " + planCompLists[1].clGenInfo[i].numData.ToString() + "\r\n");
                    debugTB.AppendText("Plan2 clGenInfo[" + i.ToString() + "] = " + planCompLists[2].clGenInfo[i].numData.ToString() + "\r\n");
                }
                if (planCompLists[1].clGenInfo[i].dataType == 2) {
                    planCompareResults.clGenResults[i] = planCompLists[1].clGenInfo[i].stringData == planCompLists[2].clGenInfo[i].stringData;
                    debugTB.AppendText("Plan1 clGenInfo[" + i.ToString() + "] = " + planCompLists[1].clGenInfo[i].stringData + "\r\n");
                    debugTB.AppendText("Plan2 clGenInfo[" + i.ToString() + "] = " + planCompLists[2].clGenInfo[i].stringData + "\r\n");
                }
            }

            //Iterate through the field info starting with index 1, since index 0 is reserved for representing the entire list.  
            //Compare either numeric or string data.  Set the results list item to the boolean of whether the two data items are equal and the 
            //logical AND of the current state of the field comparison boolean.  This way, if even one paramter compares to 'false', the result
            //for the entire field is set to 'false'.
            bool curAllFieldsBool = planCompareResults.clGenResults[0];
            debugTB.AppendText("Initial All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");

            for (int i = 0; i < compFldCnt; i++) {
                int fieldDataItems = planCompLists[1].clFieldInfo[i].Count();
                bool curFieldBool = planCompareResults.clFieldResults[i];
                debugTB.AppendText("At start of item[" + i.ToString() + "], All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");

                for (int j = 0; j < fieldDataItems; j++) {
                    if (planCompLists[1].clFieldInfo[i][j].dataType == 1) {
                        planCompareResults.clFieldResults[i] = curFieldBool & planCompLists[1].clFieldInfo[i][j].numData == planCompLists[2].clFieldInfo[i][j].numData;
                        debugTB.AppendText("Plan1 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + planCompLists[1].clFieldInfo[i][j].numData.ToString() + "\r\n");
                        debugTB.AppendText("Plan2 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + planCompLists[2].clFieldInfo[i][j].numData.ToString() + "\r\n");
                    }
                    if (planCompLists[1].clFieldInfo[i][j].dataType == 2) {
                        planCompareResults.clFieldResults[i] = curFieldBool & planCompLists[1].clFieldInfo[i][j].stringData == planCompLists[2].clFieldInfo[i][j].stringData;
                        debugTB.AppendText("Plan1 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + planCompLists[1].clFieldInfo[i][j].stringData + "\r\n");
                        debugTB.AppendText("Plan2 clFieldInfo[" + i.ToString() + "][" + j.ToString() + "] = " + planCompLists[2].clFieldInfo[i][j].stringData + "\r\n");
                    }
                    curFieldBool = planCompareResults.clFieldResults[i];
                    debugTB.AppendText("Bool for item[" + i.ToString() + "][" + j.ToString() + "] = " + curFieldBool.ToString() + "\r\n");
                }
                debugTB.AppendText("At end of item[" + i.ToString() + "], the field Bool is" + curFieldBool.ToString() + "\r\n");

                //Update the cumulative result to the clGenResults[0] to track the all-fields result.
                curAllFieldsBool = curAllFieldsBool & curFieldBool;
                debugTB.AppendText("At end of item[" + i.ToString() + "], All-Fields Bool is" + curAllFieldsBool.ToString() + "\r\n");
            }
            
            planCompareResults.clGenResults[0] = curAllFieldsBool;

        }


        //public Object GetRectControlByName_General(string objName)
        //{
        //    foreach (FrameworkElement winControl in this.panelCompCheckboxes.Children) {
        //        this.tbTemp.AppendText(", " + winControl.Name + "\r\n");

        //        if (winControl.Name == objName) {
        //            return winControl;
        //        }
        //    }

        //    return null;
        //}


        //public Object GetRectControlByName_FieldList(string objName)
        //{
        //    foreach (FrameworkElement winControl in this.grid_FieldsOkay.Children) {
        //        this.tbTemp.AppendText(", " + winControl.Name + "\r\n");

        //        if (winControl.Name == objName) {
        //            return winControl;
        //        }
        //    }

        //    return null;
        //}


    }
}
