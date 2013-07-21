using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using staunchAuthorize;
using AuthorizeNet;


namespace ANet_TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //set example card not present data
            cardNumber.Text = "4111111111111111";
            purchaseDesc.Text = "Test Purchase";

            expMonth.SelectedIndex = 11;
            expYear.SelectedIndex = 5;

            //set example card present data
            track1Box.Text = "%B4111111111111111^CARDUSER/JOHN^1803101000000000020000831000000?";
            track2Box.Text = ";4111111111111111=1803101000020000831?";

            //default API keys
            cnpLogin.Text = "768TFP5kmT";
            cnpKey.Text = "79b3Xy25RvUm568Q";

            cpLogin.Text = "459acLGGP9jf";
            cpKey.Text = "5U3Z6N36DPD8fcdv";

            batchUsername.Text = "Blackraven5468";
            batchPassword.Text = "Staunch546";
            batchDataBox.Text = "\"11111\",\"Door welding kit\",\"99.00\",\"CC\",\"AUTH_CAPTURE\", , ,\"341809871982171\",\"0214\", , , ,\"BDUKE001\",\"Bo\" ,\"Duke\", ,\"555 DukeFarm Road\",\"Hazzard County\",\"GA\",\"30603\",\"(404)555-1234\",\"(404)555-4321\",\"bo@dukefarm.com\"\r\n\"11111\",\"Door welding kit\",\"99.00\",\"CC\",\"AUTH_CAPTURE\", , ,\"345320081001741\",\"0214\", , , ,\"BDUKE001\",\"Bo\" ,\"Duke\", ,\"555 DukeFarm Road\",\"Hazzard County\",\"GA\",\"30603\",\"(404)555-1234\",\"(404)555-4321\",\"bo@dukefarm.com\"\r\n\"11111\",\"Door welding kit\",\"99.00\",\"CC\",\"AUTH_CAPTURE\", , ,\"373029757552433\",\"0214\", , , ,\"BDUKE001\",\"Bo\" ,\"Duke\", ,\"555 DukeFarm Road\",\"Hazzard County\",\"GA\",\"30603\",\"(404)555-1234\",\"(404)555-4321\",\"bo@dukefarm.com\"";
        }

        private void processNotPresent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal total = decimal.Parse(purchaseAmount.Text);
                string exp = expMonth.Text + expYear.Text;
                string card = cardNumber.Text;
                string desc = purchaseDesc.Text;

                //process card
                var auth = new staunchAuthorizeNet(cnpLogin.Text, cnpKey.Text);
                var response = auth.cardNotPresent(card, exp, total, desc);

                //show output
                debugApproved.Text = response.Approved.ToString();
                debugMessage.Text = response.Message;
                debugAuth.Text = response.AuthorizationCode;
                debugTransaction.Text = response.TransactionID;

            }
            catch (Exception ex)
            {
                debugMessage.Text = "Error converting purchase amount";
            }
        }

        private void processPresent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal total = decimal.Parse(purchaseAmount.Text);

                //process card
                var auth = new staunchAuthorizeNet(cpLogin.Text, cpKey.Text);
                var response = auth.cardPresent(total, track1Box.Text, track2Box.Text);

                //show output
                debugApproved.Text = response.Approved.ToString();
                debugMessage.Text = response.Message;
                debugAuth.Text = response.AuthorizationCode;
                debugTransaction.Text = response.TransactionID;

            }
            catch (Exception ex)
            {
                debugMessage.Text = "Error converting purchase amount";
            }
        }

        private void batchInitialize_Click(object sender, RoutedEventArgs e)
        {
            var batchWindow = new batchUploads();

            var auth = new staunchAuthorizeNet();
            staunchAuthorizeNet.BatchResult batchResult = auth.BatchUpload(batchUsername.Text, batchPassword.Text, batchDataBox.Text);

            if (batchResult.success)
            {
                debugApproved.Text = "Batch Success";
                debugMessage.Text = batchResult.result;
                debugTransaction.Text = batchResult.id;
                debugAuth.Text = batchResult.count;
            }
            else
            {
                debugApproved.Text = "Batch Failed";
                debugMessage.Text = batchResult.result;
                debugTransaction.Text = "";
                debugAuth.Text = "";
            }

            batchWindow.Show();
            batchWindow.debugOutput.Text = batchResult.debug;

            
        }
    }
}
