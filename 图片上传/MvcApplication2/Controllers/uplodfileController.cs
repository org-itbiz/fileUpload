using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication2.Controllers
{
    public class uplodfileController : Controller
    {
        //
        // GET: /Default1/B

        public ActionResult Index()
        {
            return View();
        }
         const string StrJsonTypeTextHtml = "text/html";
        private const string UploadVideoFilePath = "UploadFiles/Videos/";
        private const string UploadImageFilePath = "UploadFiles/Images/";
        private const string UploadFileCombineCharacter = "_";
        private const string UploadFileDateTimeFormat = "yyyyMMddHHmm";


        public ActionResult Uploadfile(string path)
        {
 
            try
            {
                string thumbNailPath = "";
 
                string allFilesName = string.Empty;
 
                string targetFilePath = string.Empty;
 
                string serverFileName = string.Empty;
 
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    if (Request.Files[i] != null)
                    {
 
                        FileInfo fi = new FileInfo(Request.Files[i].FileName);
 
                        //serverFileName = Request.Files[i].FileName.Substring(0, Request.Files[i].FileName.IndexOf('.')) + UploadFileCombineCharacter + DateTime.Now.ToString(UploadFileDateTimeFormat) + fi.Extension;
 
                        serverFileName = Guid.NewGuid().ToString().Replace("-", "") + fi.Extension;
 
                        if (Request.Files[i].ContentType.Contains("video"))
                        {
                            targetFilePath = UploadVideoFilePath + path + "/";
                        }
                        if (Request.Files[i].ContentType.Contains("image"))
                        {
                            targetFilePath = UploadImageFilePath + path + "/";
                            thumbNailPath = UploadImageFilePath + path + "custormImg"+"/";
                        }
 
                        string saveDir = Server.MapPath("~/") + targetFilePath;
                        thumbNailPath = Server.MapPath("~/") + thumbNailPath;
                        //上传原文件
                        ProcessPostedFile(saveDir, serverFileName, Request.Files[i]);
                        //制作标准图片
                        string imgWidth = Request.Form[2];
                        string imgHeight = Request.Form[3];
                        if (!string.IsNullOrEmpty(imgWidth) && !string.IsNullOrEmpty(imgHeight))
                        {
                            MakeThumbnail(saveDir, thumbNailPath,int.Parse(imgWidth), int.Parse(imgHeight), "Cut", serverFileName);
                        }

                        if (string.IsNullOrEmpty(allFilesName))
                        {
                            allFilesName = serverFileName;
                        }
                        else
                        {
                            allFilesName = allFilesName + "," + serverFileName;
                        }
 
                    }
                    else
                    {
                        return Json(new { success = false, timeout = false },
                            StrJsonTypeTextHtml);
                    }
                }
                string htmlTagId = Request.Form[0];
                string progressBarId = Request.Form[1];
                return Json(new { success = true, timeout = false, target = htmlTagId, progressbarId = progressBarId, serverfileName = allFilesName, },
                    StrJsonTypeTextHtml);
            }
            catch (Exception ex)
            {
               
            }
            return Json(new { success = false, timeout = false },
                StrJsonTypeTextHtml);
        }
 
        private void ProcessPostedFile(string saveDir, string saveFullName, HttpPostedFileBase postedFile)
        {
            
            try
            {
 
                if (Directory.Exists(saveDir) == false)//如果不存在就创建file文件夹 
                {
                    Directory.CreateDirectory(saveDir);
                }


                if (postedFile == null) return;
                postedFile.SaveAs(saveDir+saveFullName);

                return ;
            }
            catch (Exception ex)
            {
             
                throw ex;
            }
        }


        /// <summary>        
        /// 生成大概想要的图片 !+_+      
        /// </summary>        
        /// <param name="originalImagePath">源图路径（物理路径）</param>        
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>        
        /// <param name="width">缩略图宽度</param>        
        /// <param name="height">缩略图高度</param>        
        /// <param name="mode">生成缩略图的方式</param>   
        /// <param name="saveFullName">文件的名字</param>           
        private void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode, string saveFullName)
        {
            if (Directory.Exists(thumbnailPath) == false)//如果不存在就创建file文件夹 
            {
                Directory.CreateDirectory(thumbnailPath);
            }
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath+saveFullName);
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            switch (mode)
            {
                case "HW":
                    //指定高宽缩放（可能变形）                                    
                    break;
                case "W":
                    //指定宽，高按比例                                        
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":
                    //指定高，宽按比例                    
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    //指定高宽裁减（不变形）                                    
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }
            //新建一个bmp图片            
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板            
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法            
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充            
            g.Clear(System.Drawing.Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分            
            g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                new System.Drawing.Rectangle(x, y, ow, oh),
                System.Drawing.GraphicsUnit.Pixel);
            try
            {
              
                //以jpg格式保存缩略图        
                bitmap.Save(thumbnailPath + saveFullName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Exception e)
            {
                throw;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

    }
}

