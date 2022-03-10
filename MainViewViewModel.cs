using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Win32;
using Prism.Commands;
using RevitAPIUI8._1.Wrappers;
using RevitAPIUILibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RevitAPIUI8._1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;
        public List<string> Formats { get; } = new List<string>();
        public string SelectedFormat { get; set; }
        public List<ViewPlan> Views { get; } = new List<ViewPlan>();
        public ViewPlan SelectedViewPlan { get; set; }
        public DelegateCommand SaveCommand { get; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;
            Formats = FormatsUtils.GetFormat(commandData);
            Views = ViewsUtils.GetFloorPlanViews(_doc);
            SaveCommand = new DelegateCommand(OnSaveCommand);
        }

        private void OnSaveCommand()
        {
            if (SelectedFormat == null && SelectedViewPlan == null)
                return;

            //RaiseHideRequest();

            try 
            {
                switch (SelectedFormat)
                {
                    case "DWG":
                        if (SelectedViewPlan == null)
                        {
                            TaskDialog.Show("Ошибка", "Выберите вид для экспорта");
                            return;
                        }
                        
                        SaveFileDialog fileSaveDwg = new SaveFileDialog();
                        fileSaveDwg.Filter = "dwg files (*.dwg)|*.dwg|All files (*.*)|*.*";
                        fileSaveDwg.RestoreDirectory = true;
                        fileSaveDwg.ShowDialog();
                        string filePathDwg = fileSaveDwg.FileName;
                        string nameDwg = Path.GetFileName(filePathDwg);
                        string pathDwg = Path.GetDirectoryName(filePathDwg);

                        if (filePathDwg == string.Empty)
                            return;

                        using (var ts = new Transaction(_doc, "Export DWG"))
                        {
                            ts.Start();
                            var dwgOption = new DWGExportOptions();
                            _doc.Export(pathDwg, nameDwg, new List<ElementId> { SelectedViewPlan.Id }, dwgOption);
                            ts.Commit();
                        }
                        break;

                    case "IFC":
                 
                        SaveFileDialog fileSaveIfc = new SaveFileDialog();
                        fileSaveIfc.Filter = "ifc files (*.ifc)|*.ifc|All files (*.*)|*.*";
                        fileSaveIfc.RestoreDirectory = true;
                        fileSaveIfc.ShowDialog();
                        string filePathIfc = fileSaveIfc.FileName;
                        string nameIfc = Path.GetFileName(filePathIfc);
                        string pathIfc = Path.GetDirectoryName(filePathIfc);

                        if (filePathIfc == string.Empty)
                            return;

                        using (var ts = new Transaction(_doc, "Export IFC"))
                        {
                            ts.Start();
                            var ifcOption = new IFCExportOptions();
                            _doc.Export(pathIfc, nameIfc, ifcOption);
                            ts.Commit();
                        }
                        break;

                    case "NWC":
                        if (SelectedViewPlan == null)
                        {
                            TaskDialog.Show("Ошибка", "Выберите вид для экспорта");
                            return;
                        }
                        SaveFileDialog fileSaveNwc = new SaveFileDialog();
                        fileSaveNwc.Filter = "nwc files (*.nwc)|*.nwc|All files (*.*)|*.*";
                        fileSaveNwc.RestoreDirectory = true;
                        fileSaveNwc.ShowDialog();
                        string filePathNwc = fileSaveNwc.FileName;
                        string nameNwc = Path.GetFileName(filePathNwc);
                        string pathNwc = Path.GetDirectoryName(filePathNwc);

                        if (filePathNwc == string.Empty)
                            return;
                        //FilteredElementCollector nwc = new FilteredElementCollector(_doc);
                        //View3D v3d = nwc.OfClass(typeof(View3D)).ToElements().Cast<View3D>().Where(v => !v.IsTemplate).First();
                        //using (var ts = new Transaction(_doc, "Export NWC"))
                        //{
                        //    ts.Start();
                            var nwcOption = new NavisworksExportOptions 
                            { 
                                ExportScope = NavisworksExportScope.View,
                                ViewId = SelectedViewPlan.Id
                            }; 
                            _doc.Export(pathNwc, nameNwc, nwcOption);
                        //    ts.Commit();
                        //}
                        break;

                    case "JPG":
                        if (SelectedViewPlan == null)
                        {
                            TaskDialog.Show("Ошибка", "Выберите вид для экспорта");
                            return;
                        }
                        SaveFileDialog fileSaveJpg = new SaveFileDialog();
                        fileSaveJpg.Filter = "jpg files (*.jpg)|*.jpg|All files (*.*)|*.*";
                        fileSaveJpg.RestoreDirectory = true;
                        fileSaveJpg.ShowDialog();
                        string filePathJpg = fileSaveJpg.FileName;
                        string nameJpg = Path.GetFileName(filePathJpg);
                        string pathJpg = Path.GetDirectoryName(filePathJpg);

                        if (filePathJpg == string.Empty)
                            return;

                        using (var ts = new Transaction(_doc, "Export JPG"))
                        {
                            ts.Start();
                            var jpgOption = new ImageExportOptions
                            {
                                ZoomType = ZoomFitType.FitToPage,
                                PixelSize = 1920,
                                FilePath = filePathJpg,
                                FitDirection = FitDirectionType.Horizontal,
                                HLRandWFViewsFileType = ImageFileType.JPEGLossless,
                                ImageResolution = ImageResolution.DPI_600,
                                ExportRange = ExportRange.SetOfViews,
                            };
                            jpgOption.SetViewsAndSheets(new List<ElementId> { SelectedViewPlan.Id });
                            _doc.ExportImage(jpgOption);
                            ts.Commit();
                        }
                        break;

                    default:
                        return;
                }
            }
            catch (Exception)
            {

            }          

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler HideRequest;
        private void RaiseHideRequest()
        {
            HideRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowRequest;
        private void RaiseShowRequest()
        {
            ShowRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
