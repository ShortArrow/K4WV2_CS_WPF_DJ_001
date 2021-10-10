using System;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Linq;

namespace K4WV2_CS_WPF_DJ_001
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //Kinect SDK
        KinectSensor kinect;

        BodyFrameReader bodyFrameReader;
        Body[] bodies;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Kinectを開く
                kinect = KinectSensor.GetDefault();
                kinect.Open();

                //ボディーリーダーを開く
                bodyFrameReader = kinect.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

                //Bodyを入れる配列を作る
                bodies = new Body[kinect.BodyFrameSource.BodyCount];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bodyFrameReader!=null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }
            if (kinect!=null)
            {
                kinect.Close();
                kinect = null;
            }
        }

        private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            UpdateBodyFrame(e);
            DrawBodyFrame();
        }

        private void DrawBodyFrame()
        {
            try
            {
                CanvasBody.Children.Clear();

                //追跡しているBodyのみループする
                foreach (var body in bodies.Where(b => b.IsTracked))
                {
                    foreach (var joint in body.Joints)
                    {
                        //手の位置が追跡状態
                        if (joint.Value.TrackingState == TrackingState.Tracked)
                        {
                            DrawEllipse(joint.Value, 10, Brushes.Blue);
                        }
                        //手の位置が推測状態
                        else if (joint.Value.TrackingState == TrackingState.Inferred)
                        {
                            DrawEllipse(joint.Value, 10, Brushes.Red);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
            
        }

        private void DrawEllipse(Joint joint, int R, Brush brush)
        {
            var ellipse = new Ellipse()
            {
                Width = R,
                Height = R,
                Fill = brush,
            };

            //カメラ座標系をDepth座標系に変換する
            var point = kinect.CoordinateMapper.MapCameraPointToDepthSpace(joint.Position);
            if ((point.X < 0) || (point.Y < 0))
            {
                return;
            }

            //Depth座標系で円を配置する
            Canvas.SetLeft(ellipse, point.X - (R / 2));
            Canvas.SetTop(ellipse, point.Y - (R / 2));

            CanvasBody.Children.Add(ellipse);
        }

        private void UpdateBodyFrame(BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame=e.FrameReference.AcquireFrame())
            {
                if (bodyFrame==null)
                {
                    return;
                }

                //ボディーデータを取得する
                bodyFrame.GetAndRefreshBodyData(bodies);
            }
        }
    }
}
