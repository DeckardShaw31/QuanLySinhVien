using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace ManagementStudentFirebase
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Declare

        DataTable dt = new DataTable();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "opvPdDshiA5ZWttVVilmsxJPJWeuIndiNuUkL3H2",
            BasePath = "https://managementstudent-8d677-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        #endregion

        #region Functions

        void Connecting()
        {
            txtHoTen.Focus();
            client = new FireSharp.FirebaseClient(config);
            //thêm headercolumn cho datagridview
            dt.Columns.Add("Mã");
            dt.Columns.Add("Họ và tên");
            dt.Columns.Add("Lớp");
            dt.Columns.Add("Ngày sinh");
            dt.Columns.Add("Giới tính");
            dt.Columns.Add("Địa chỉ");

            dtgDSHS.DataSource = dt;
        }

        private async void export()
        {
            //TRước khi lấy danh sách, xóa danh sách đang có trong DataGridView
            dt.Rows.Clear();
            int i = 0;
            //lấy tổng học sinh ở trên Firebase mục "SiSo"
            FirebaseResponse resp1 = await client.GetTaskAsync("SiSo");
            Counter obj1 = resp1.ResultAs<Counter>();

            int cnt = Convert.ToInt32(obj1.cnt);

            //Sử dụng vòng lặp while lặp đến tổng học sinh đang có
            while (true)
            {
                if (i == cnt)
                {
                    break;
                }
                i++;
                try
                {
                    //kết nối tới firebase và lấy danh sách về
                    FirebaseResponse resp2 = await client.GetTaskAsync("QuanLyHocSinh/" + i);
                    Data obj2 = resp2.ResultAs<Data>();

                    DataRow row = dt.NewRow();

                    row["Mã"] = obj2.MSHS;
                    row["Họ và tên"] = obj2.HoTen;
                    row["Lớp"] = obj2.Lop;
                    row["Ngày sinh"] = obj2.NgaySinh;
                    row["Địa chỉ"] = obj2.DiaChi;
                    row["Giới tính"] = obj2.GioiTinh;
                    // dùng phương thức Add() để đổ dữ liệu vào Datatable
                    dt.Rows.Add(row);
                    //hiển thị tổng học sinh
                    Total();
                }
                catch
                {

                }
            }
        }

        private void Total()
        {
            int count = 0;
            if (dtgDSHS.Rows.Count <= 0)
            {
                txtTongHS.Text = "0";
            }
            else
            {
                count = Convert.ToInt32(dtgDSHS.Rows.Count);
                txtTongHS.Text = count.ToString();
            }
        }

        void BindingListStudent()
        {
            txtMaHS.DataBindings.Add(new Binding("Text", dtgDSHS.DataSource, "Mã", true, DataSourceUpdateMode.Never));
            txtHoTen.DataBindings.Add(new Binding("Text", dtgDSHS.DataSource, "Họ và tên", true, DataSourceUpdateMode.Never));
            txtLop.DataBindings.Add(new Binding("Text", dtgDSHS.DataSource, "Lớp", true, DataSourceUpdateMode.Never));
            txtDiaChi.DataBindings.Add(new Binding("Text", dtgDSHS.DataSource, "Địa chỉ", true, DataSourceUpdateMode.Never));
            dtpickerNgaySinh.DataBindings.Add(new Binding("Value", dtgDSHS.DataSource, "Ngày Sinh", true, DataSourceUpdateMode.Never));
            cboGioiTinh.DataBindings.Add(new Binding("Text", dtgDSHS.DataSource, "Giới Tính", true, DataSourceUpdateMode.Never));
        }

        void Reset()
        {
            txtHoTen.Text = "";
            txtMaHS.Text = "";
            txtLop.Text = "";
            txtDiaChi.Text = "";
            cboGioiTinh.Text = "Nam";
            txtHoTen.Focus();
        }

        private async void AddStudent()
        {
            //kiểm tra thông tin của học sinh rồi mới thực hiện thêm mới
            if (txtHoTen.Text == "")
            {
                MessageBox.Show("Vui lòng nhập họ và tên học sinh", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtHoTen.Focus();
            }
            else if (txtLop.Text == "")
            {
                MessageBox.Show("Vui lòng nhập lớp cho học sinh", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtLop.Focus();
            }
            else if (txtDiaChi.Text == "")
            {
                MessageBox.Show("Vui lòng nhập địa chỉ của học sinh", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtDiaChi.Focus();
            }
            else
            {
                //lấy số lượng tổng học sinh
                FirebaseResponse resp = await client.GetTaskAsync("SiSo");
                Counter get = resp.ResultAs<Counter>();
                //khởi tạo một object thuộc Class Data
                var data = new Data
                {
                    MSHS = (Convert.ToInt32(get.cnt) + 1).ToString(),
                    HoTen = txtHoTen.Text,
                    Lop = txtLop.Text,
                    NgaySinh = dtpickerNgaySinh.Text.ToString(),
                    DiaChi = txtDiaChi.Text,
                    GioiTinh = cboGioiTinh.Text
                };

                //Đẩy dữ liệu lên Firebase
                SetResponse response = await client.SetTaskAsync("QuanLyHocSinh/" + data.MSHS, data);
                Data result = response.ResultAs<Data>();

                MessageBox.Show("Đã thêm mới thành công học sinh có mã " + result.MSHS);
                Reset();

                var obj = new Counter
                {
                    cnt = data.MSHS
                };

                SetResponse response1 = await client.SetTaskAsync("SiSo", obj);
                export();
            }
        }

        private async void EditStudent()
        {
            if (txtMaHS.Text == "")
            {
                MessageBox.Show("Vui lòng chọn học sinh cần sửa thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                FirebaseResponse resp1 = await client.GetTaskAsync("SiSo");
                Counter obj1 = resp1.ResultAs<Counter>();

                int cnt = Convert.ToInt32(obj1.cnt);
                if (Convert.ToInt32(txtMaHS.Text) > cnt)
                {
                    MessageBox.Show("Không tìm thấy học sinh cần sửa thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMaHS.Text = "";
                    txtMaHS.Focus();
                }
                else
                {
                    var data = new Data
                    {
                        MSHS = txtMaHS.Text,
                        HoTen = txtHoTen.Text,
                        Lop = txtLop.Text,
                        NgaySinh = dtpickerNgaySinh.Text.ToString(),
                        DiaChi = txtDiaChi.Text,
                        GioiTinh = cboGioiTinh.Text
                    };

                    FirebaseResponse response = await client.UpdateTaskAsync("QuanLyHocSinh/" + txtMaHS.Text, data);
                    Data result = response.ResultAs<Data>();
                    MessageBox.Show("Sửa thành công học sinh có mã: " + result.MSHS, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Reset();
                    export();
                }
            }
        }

        private async void DeleteStudent()
        {
            DialogResult dg = MessageBox.Show("Bạn có chắc chắn xóa hết ?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dg == DialogResult.Yes)
            {
                FirebaseResponse response = await client.DeleteTaskAsync("QuanLyHocSinh");
                MessageBox.Show("Đã xóa hết học sinh thành công !!! ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                FirebaseResponse resp = await client.GetTaskAsync("SiSo");
                Counter get = resp.ResultAs<Counter>();

                var obj = new Counter
                {
                    cnt = "0"
                };

                SetResponse response1 = await client.SetTaskAsync("SiSo", obj);
                Reset();
                export();
                txtTongHS.Text = "0";
            }
        }

        private void ToExcel(DataGridView dataGridView1, string fileName)
        {
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook workbook;
            Microsoft.Office.Interop.Excel.Worksheet worksheet;

            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = false;
                excel.DisplayAlerts = false;

                workbook = excel.Workbooks.Add(Type.Missing);

                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets["Sheet1"];
                worksheet.Name = "Quản lý học sinh";

                // export header
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
                }
                // export content
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                    }
                }
                // save workbook
                workbook.SaveAs(fileName);
                workbook.Close();
                excel.Quit();
                MessageBox.Show("Xuất dữ liệu ra Excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                workbook = null;
                worksheet = null;
            }
        }

        #endregion

        #region Events

        private void Form1_Load(object sender, EventArgs e)
        {
            Connecting();
            export();
            BindingListStudent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult dg = MessageBox.Show("Bạn có chắc chắn muốn thoát ?", "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if(dg == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void btnLayThongTin_Click(object sender, EventArgs e)
        {
            export();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            AddStudent();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            EditStudent();
        }

        private void btnXoaTatCa_Click(object sender, EventArgs e)
        {
            DeleteStudent();
        }

        private void txtLocDS_TextChanged(object sender, EventArgs e)
        {
            string rowFilter = string.Format("{0} like '{1}'", "Mã", "*" + txtLocDS.Text + "*");
            (dtgDSHS.DataSource as DataTable).DefaultView.RowFilter = rowFilter;
            Total();
        }

        private void xuatDSHS_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ToExcel(dtgDSHS, saveFileDialog1.FileName);
            }
        }

        #endregion
    }
}
