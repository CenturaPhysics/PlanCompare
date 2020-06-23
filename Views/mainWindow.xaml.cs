using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;

namespace PlanCompare.Views
{
    /// <summary>
    /// Interaction logic for mainWindow.xaml
    /// </summary>
    public partial class mainWindow : Window
    {
        public ScriptContext theContext;
        public DataManager theDM;


        //Create arrays of lists of TextBlock objects.  These will hold the TextBlock controls for the plan information
        //for each plan.  This gives us a way to update the values of these TextBlocks without needing to repeatedly
        //access the MainWindow children collection.  And, by organizing it by plan, we can create a function to update
        //these that just gets sent the plan num to select the right list of controls.  We create 3 each, and ingnore
        //the index 0 items, so that we can use 1 and 2 as indexes for plans 1 and 2.
        List<TextBlock>[] planGenTBs = new List<TextBlock>[3];
        List<TextBlock>[] planFieldTBs = new List<TextBlock>[3];

        //Create a list of rectangles used to display general plan comparison results. As above, this list is made just to
        //provide an easier way of access these rectangles, rather than always going to the Window child control collection.
        List<Rectangle> rectsGen = new List<Rectangle>();

        //Create a list of rectangles used to display field plan comparison results. As above, this list is made just to
        //provide an easier way of access these rectangles, rather than always going to the Window child control collection.
        List<Rectangle> rectsField = new List<Rectangle>();


        //Empty constructor for MainWindow
        public mainWindow()
        {
            InitializeComponent();

            theDM = new DataManager();

            //Initialize our lists of plan general info TextBlocks for easy updating of plan info later.
            //Set index 0 to null, so that we can keep the index matching the actual plan number for readability.
            planGenTBs[0] = null;

            planGenTBs[1] = new List<TextBlock>();
            planGenTBs[1].Add(this.grid_tb_CourseId1);
            planGenTBs[1].Add(this.grid_tb_PlanId1);
            planGenTBs[1].Add(this.grid_tb_NoOfF1);
            planGenTBs[1].Add(this.grid_tb_Alg1);

            planGenTBs[2] = new List<TextBlock>();
            planGenTBs[2].Add(this.grid_tb_CourseId2);
            planGenTBs[2].Add(this.grid_tb_PlanId2);
            planGenTBs[2].Add(this.grid_tb_NoOfF2);
            planGenTBs[2].Add(this.grid_tb_Alg2);

            planFieldTBs[0] = null;
            planFieldTBs[1] = new List<TextBlock>();
            planFieldTBs[2] = new List<TextBlock>();

            //Add the general results rectangles to the list of general results markers.  Add the All-Fields result first,
            //so that it is always item 0, and thus we will always know where to find it.
            rectsGen.Add(this.all_flds_okay);
            rectsGen.Add(this.compNumOfFields);

            theDM.debugTB = this.tbDebug;
        }


        //Sets the current scroll location of the scrollview control to be equal to the scroll bar position.  This allows
        //the scroll bar to drive the scroll viewer.
        public void ScrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            this.svPlan1.ScrollToVerticalOffset(this.sbPlanCompare.Value);
        }


        //This function is used to initialize various controls with the current plan data.  This event is fired last in
        //the chain of events that occur when a window is created.  See: https://wpf.2000things.com/tag/window-events/
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.sbPlanCompare.Maximum = (this.svPlan1.ExtentHeight - this.svPlan1.ActualHeight);

            //Code to initialize the Course and Plan drop-downs... if needed.

            //Set the second course combobox to match the first course combobox
            this.cbCourses2.SelectedIndex = this.cbCourses1.SelectedIndex;

            //Set the plan2 combobox to blank.  Set the plan1 combobox to the first item in it's list.
            //NOTE:  seting the SelectedIndex for any of these combobox controls fires their SelectionChanged events.
            this.cbPlans2.SelectedIndex = -1;  // would fire cbPlans2_SelectionChanged, but we have left that blank for now.
            this.cbPlans1.SelectedIndex = 0;  //immediately fires cbPlans1_SelectionChanged

            //Set the initial two rows of the field data grid to height 0.  These rows are the header, and gray separator bar.
            this.grid_FieldData.RowDefinitions[0].Height = new GridLength(0);
            this.grid_FieldData.RowDefinitions[1].Height = new GridLength(0);

            //Set the initial two rows of the field okay grid to height 0.  These rows are both blank.
            this.grid_FieldsOkay.RowDefinitions[0].Height = new GridLength(0);
            this.grid_FieldsOkay.RowDefinitions[1].Height = new GridLength(0);
        }


        //Fired when the course selection combobox is changed.  First sets the plan selection combobox to -1, so that it's
        //selection changed events don't end up causing an "object not found error".
        public void cbCourses1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ( this.cbCourses1.SelectedIndex != -1 ) {
                if ( (this.cbCourses1.SelectedItem as Course) != null ) {
                    this.cbPlans1.SelectedIndex = -1;
                    this.cbPlans1.ItemsSource = (this.cbCourses1.SelectedItem as Course).PlanSetups;
                    this.cbPlans1.SelectedIndex = 0;

                    //Also, assume that if the first plan is changed, then the user will want to select a different course 2 and
                    //plan 2, so set both of those comboboxes to -1 as well.  Set the Course 2 combobox to match the Course 1 combobox.
                    this.cbPlans2.SelectedIndex = -1;
                    this.cbCourses2.SelectedIndex = this.cbCourses1.SelectedIndex;
                }
            }
            else {
                theDM.ClearPlanData(1);
            }
        }


        //Same as above for course 1 combobox.  In this case, don't wipe out all plan 1 data structures.
        public void cbCourses2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cbCourses2.SelectedIndex != -1) {
                if ( (this.cbCourses2.SelectedItem as Course) != null ) {
                    this.cbPlans2.SelectedIndex = -1;
                    this.cbPlans2.ItemsSource = (this.cbCourses2.SelectedItem as Course).PlanSetups;
                    this.cbPlans2.SelectedIndex = -1;
                }
            }
            else {
                theDM.ClearPlanData(2);
            }
        }


        //Fired when the plan selection combobox is changed.  This can happen when the course selection is changed, and we don't
        //want to respond to that, or we will end up trying to load a plan object that is null.  So, we check to see if this combobox
        //is set to -1.  If so, we ignore the update, and clear plan data if needed (since no plan is selected).
        public void cbPlans1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Only respond if the plan comobox selected index is not -1.
            if (this.cbPlans1.SelectedIndex != -1) {
                if ( (this.cbPlans1.SelectedItem as PlanSetup) != null ) {
                    theDM.thePlans[1] = (this.cbPlans1.SelectedItem as PlanSetup);
                    theDM.theCourses[1] = (this.cbCourses1.SelectedItem as Course);

                    //Since the selected plan has changed, clear all plan data,  the field data grid, and the field okay grid, to allow for
                    //adding new rows for the new plan fields.
                    theDM.ClearPlanData(1);

                    if (this.grid_FieldData.RowDefinitions.Count > 2) { RemoveFieldGridRows(); }
                    AddFieldGridRows(theDM.thePlans[1].Beams.Count());
                    //this.UpdateLayout();
                    this.grid_FieldData.UpdateLayout();
                    this.grid_FieldsOkay.UpdateLayout();

                    //Call the Data Manager function that updates the field data.  We send the Grid object and TextBlock list as the targets
                    //of the update.  This way, if we change the interface, we can just send the new targets without re-writing the data manager.
                    theDM.SetPlanData(1, this.grid_FieldData, planGenTBs[1], planFieldTBs[1], 0);
                    
                    //Lastly, since we changed plan 1, assume that we will need a different plan 2, so clear all of the old plan 2 data.
                    

                }
                else {
                    MessageBox.Show("Warning: Current PlanSetup for selected 'plan 1' is null.  Cannot use.");
                }
            }
            else {
                //If plan1 selected index = -1, then no plan is selected.  Check to see if there are field data grid rows.
                //If there are, then remove them.  Also, send the ClearPlanData command to the Data Manager.
                if (this.grid_FieldData.RowDefinitions.Count > 2) {
                    theDM.ClearPlanData(1);
                    RemoveFieldGridRows();
                }
            }
        }


        //Same as above for plan 1 combobox.
        public void btn_LoadPlan2_Click(object sender, RoutedEventArgs e)
        {
            Course selCourse1 = (this.cbCourses1.SelectedItem as Course);
            Course selCourse2 = (this.cbCourses2.SelectedItem as Course);
            PlanSetup selPlan1 = (this.cbPlans1.SelectedItem as PlanSetup);
            PlanSetup selPlan2 = (this.cbPlans2.SelectedItem as PlanSetup);

            //Only respond if the plan comobox selected index is not -1.
            if (this.cbPlans2.SelectedIndex != -1) {
                if (selCourse1.Id != selCourse2.Id | selPlan1.Id != selPlan2.Id) {
                    if ( (this.cbPlans2.SelectedItem as PlanSetup) != null ) {
                        theDM.thePlans[2] = (this.cbPlans2.SelectedItem as PlanSetup);
                        theDM.theCourses[2] = (this.cbCourses2.SelectedItem as Course);

                        int curFieldRows = this.grid_FieldData.RowDefinitions.Count() - 2;
                        int plan2FieldCount = theDM.thePlans[2].Beams.Count();
                        if ( plan2FieldCount > curFieldRows ) {
                            AddFieldGridRows( plan2FieldCount - curFieldRows );
                        }

                        //Call the Data Manager function that updates the field data.  We send the Grid object and TextBlock list as the targets
                        //of the update.  This way, if we change the interface, we can just send the new targets without re-writing the data manager.
                        ClearFieldTBsForPlan(2);
                        theDM.SetPlanData(2, this.grid_FieldData, planGenTBs[2], planFieldTBs[2], 9);
                        UpdateFieldOkayResults();
                    }
                    else {
                        MessageBox.Show("Warning:  Current PlanSetup for selected 'plan 2' is null.  Cannot use.");
                    }
                }
                else {
                    ClearFieldTBsForPlan(2);
                    theDM.ClearPlanData(2);
                }
            }
        }


        public void AddFieldGridRows(int numOfRows)
        {
            //Create new RowDefinition and then, for each field in the plan, add a row to both the field_data grid
            //and the field_okay grid.  For the field_okay grid, add a rectangle for the comparison marker.
            RowDefinition newRow = new RowDefinition();
            for (int i = 0; i < numOfRows; i++) {
                //MessageBox.Show("Made it into the if statements of AddFieldGridRows.");
                this.grid_FieldData.RowDefinitions.Add(new RowDefinition());
                this.grid_FieldsOkay.RowDefinitions.Add(new RowDefinition());

                //Check if the field expander is currently expanded.  If so, set row height to 24. If not, set to 0.
                if(this.fieldGridExpander.IsExpanded) {
                    this.grid_FieldData.RowDefinitions[2 + i].Height = new GridLength(24);
                    this.grid_FieldsOkay.RowDefinitions[2 + i].Height = new GridLength(24);
                }
                else {
                    this.grid_FieldData.RowDefinitions[2 + i].Height = new GridLength(0);
                    this.grid_FieldsOkay.RowDefinitions[2 + i].Height = new GridLength(0);
                }

                //Create new rectangles for the field_okay grid.  Set the color to Green.
                Rectangle newRect = new Rectangle();
                newRect.Height = 20;
                newRect.Width = 20;
                newRect.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                newRect.Margin = new Thickness(0, 4, 0, 4);
                newRect.Name = "compF" + i.ToString();
                this.grid_FieldsOkay.Children.Add(newRect);
                Grid.SetRow(newRect, 1 + i);
                Grid.SetColumn(newRect, 0);

                //Add the rectangle to the rectsField list.
                this.rectsField.Add(newRect);

                //This is just added for debuging purposes.  Okay to delete once this function is validated.
                //this.tbDebug.AppendText("Rect Name for Field " + i.ToString() + " is: " + newRect.Name + "\r\n");
            }
        }


        public void RemoveFieldGridRows()
        {
            //First, get the number or rows in the field data grid
            int curNumOfRows = this.grid_FieldData.RowDefinitions.Count;
            int curNumOfChildren = this.grid_FieldData.Children.Count;
            //Check to see if there are rows that need to be removed.  If so, remove all but the first two rows.  
            //The first two rows are the header and separator bar, which are rows 0 and 1 respectively.
            if (curNumOfRows > 2) {
                //Next, check each child control.  If it's row number is greater than 1, remove it.
                //We have to use a standard FOR loop here, as FOREACH is not allowed if we are altering the collection as we iterate.
                for (int i = (curNumOfChildren - 1); i >= 0; i--) {
                    int tbRow = Grid.GetRow(this.grid_FieldData.Children[i]);
                    if (tbRow > 1) { this.grid_FieldData.Children.Remove(this.grid_FieldData.Children[i]); }
                }
                //Next, remove the grid rows themselves... starting from the highest index, working our way down to row 1, where we stop.
                for (int i = (curNumOfRows - 1); i > 1; i--) {
                    this.grid_FieldData.RowDefinitions.RemoveAt(i);
                }
            }

            //Repeat for the field okay grid...
            curNumOfRows = this.grid_FieldsOkay.RowDefinitions.Count();

            //Clear the rectsField list
            rectsField.Clear();

            if (curNumOfRows > 2) {
                //For the field okay grid, children are easy, because in it's default state... there shouldn't be any children.
                //So, if we need to remove grid rows, we just clear all children.
                this.grid_FieldsOkay.Children.Clear();
                //Next, remove the grid rows themselves... starting from the highest index, working our way down to row 1, where we stop.
                for (int i = (curNumOfRows - 1); i > 1; i--) {
                    this.grid_FieldsOkay.RowDefinitions.RemoveAt(i);
                }
            }

        }


        //Deletes all the TextBlock controls in the field data grid that are associated with the plan.
        public void ClearFieldTBsForPlan(int planNum)
        {
            int cnt = planFieldTBs[planNum].Count();
           
            for ( int i = cnt -1; i>=0; i-- ) {
                this.grid_FieldData.Children.Remove( planFieldTBs[planNum][i] );
            }

            planFieldTBs[planNum].Clear();
            this.grid_FieldData.UpdateLayout();
        }


        public void UpdateFieldOkayResults()
        {
            int cntGen = theDM.planCompareResults.clGenResults.Count();
            int cntFld = theDM.planCompareResults.clFieldResults.Count();

            for (int i=0; i<cntGen; i++) {
                if(theDM.planCompareResults.clGenResults[i] == true) {
                    rectsGen[i].Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                else {
                    rectsGen[i].Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
            }

            for (int i = 0; i < cntFld; i++) {
                if (theDM.planCompareResults.clFieldResults[i] == true) {
                    rectsField[i].Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                else {
                    rectsField[i].Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
            }
            this.UpdateLayout();
        }


        //An interface function for expanding the field data list.  Simply changes the grid heights for the field_data and
        //field_okay grids to their nominal values.
        public void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            int cnt = (this.cbPlans1.SelectedItem as PlanSetup).Beams.Count();
            this.grid_FieldData.RowDefinitions[0].Height = new GridLength(24);
            this.grid_FieldData.RowDefinitions[1].Height = new GridLength(4);
            this.grid_FieldsOkay.RowDefinitions[0].Height = new GridLength(24);
            for (int i=0; i<cnt; i++) {
                this.grid_FieldData.RowDefinitions[2 + i].Height = new GridLength(24);
                this.grid_FieldsOkay.RowDefinitions[1 + i].Height = new GridLength(24);
            }

            //Check_Comparison();
        }


        //An interface function for collapsing the field data list.  Simply changes the grid heights for the field_data and
        //field_okay grids to zero (0).  Note, the grid rows and all their controls are still there, you just can't see them.
        public void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            int cnt = (this.cbPlans1.SelectedItem as PlanSetup).Beams.Count();
            this.grid_FieldData.RowDefinitions[0].Height = new GridLength(0);
            this.grid_FieldData.RowDefinitions[1].Height = new GridLength(0);
            for (int i = 0; i < cnt; i++) {
                this.grid_FieldData.RowDefinitions[2 + i].Height = new GridLength(0);
                this.grid_FieldsOkay.RowDefinitions[1 + i].Height = new GridLength(0);
            }
        }


        private void btn_TestRowDelete_Click(object sender, RoutedEventArgs e)
        {
            RemoveFieldGridRows();
        }


        public void PostComparisonToDebug()
        {
            int cntGen = theDM.planCompareResults.clGenResults.Count();
            int cntFld = theDM.planCompareResults.clFieldResults.Count();

            for(int i=0; i<cntGen; i++) {
                this.tbDebug.AppendText("General comparison results for item " + i.ToString() + " is: " + theDM.planCompareResults.clGenResults[i].ToString() + "\r\n");
            }
            for (int i = 0; i < cntFld; i++) {
                this.tbDebug.AppendText("Field comparison results for item " + i.ToString() + " is: " + theDM.planCompareResults.clFieldResults[i].ToString() + "\r\n");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PostComparisonToDebug();
        }
    }
}
