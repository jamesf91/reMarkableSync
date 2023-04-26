using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RemarkableSync.OnenoteAddin
{
    public partial class PreviewForm : Form
    {
        private List<int> _selectedBitmaps = new List<int>();
        public List<int> SelectedBitmaps
        {
            get { return _selectedBitmaps; }
        }
        public PreviewForm(List<Bitmap> bitmaps)
        {
            InitializeComponent();

            lvPreviews.View = View.LargeIcon;
            lvPreviews.LargeImageList = ilPreviews;
            lvPreviews.LargeImageList.ImageSize = new Size(140, 180);

            foreach (Bitmap bitmap in bitmaps)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = lvPreviews.LargeImageList.Images.Count;
                lvPreviews.LargeImageList.Images.Add(bitmap);
                lvPreviews.Items.Add(item);
            }


        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            _selectedBitmaps.Clear();
            foreach (ListViewItem item in lvPreviews.SelectedItems)
            {
                _selectedBitmaps.Add(item.ImageIndex);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
