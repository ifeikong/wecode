﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

namespace WeCode1._0
{
    public partial class YouDaoTree : DockContent
    {
        public FormMain formParent;

        public YouDaoTree()
        {
            InitializeComponent();
        }

        //窗体加载
        private void YouDaoTree_Load(object sender, EventArgs e)
        {
            //绑定树
            if (Attachment.IsTokeneffective == 1)
            {
                treeViewYouDao.Enabled = true;
                IniYoudaoTree();
            }
            else
            {
                treeViewYouDao.Enabled = false;
            }
        }



        #region 从xml加载生成树
        /// <summary>
        /// 从XML加载绑定树
        /// </summary>
        public void IniYoudaoTree()
        {
            this.treeViewYouDao.Enabled = true;
            try
            {
                this.Cursor = Cursors.WaitCursor;

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("TreeNodeLocal.xml");

                treeViewYouDao.Nodes.Clear();

                treeViewYouDao.ImageList = imageList1;

                XmlNode wecode = xDoc.DocumentElement;

                foreach (XmlNode cNode in wecode)
                {
                    //添加根节点
                    TreeNode tNode = new TreeNode();
                    tNode.ImageIndex = 0;
                    tNode.SelectedImageIndex = 0;
                    tNode.Text = cNode.Attributes["title"].Value.ToString();
                    tNode.Name = cNode.Attributes["id"].Value;
                    if (cNode.Name == "note")
                    {
                        treeTagNote tNoteTag = new treeTagNote();
                        tNoteTag.path = cNode.Attributes["path"].Value;
                        tNoteTag.createtime = cNode.Attributes["createtime"].Value;
                        tNoteTag.updatetime = cNode.Attributes["updatetime"].Value;
                        tNoteTag.Language = cNode.Attributes["Language"].Value;
                        tNoteTag.isMark = cNode.Attributes["isMark"].Value;

                        tNode.ImageIndex = 1;
                        tNode.SelectedImageIndex = 1;
                        tNode.Tag = tNoteTag;
                    }
                    else if (cNode.Name == "book")
                    {
                        treeTagBook tBookTag = new treeTagBook();
                        tBookTag.Language = cNode.Attributes["Language"].Value;
                        tNode.Tag = tBookTag;
                    }

                    treeViewYouDao.Nodes.Add(tNode);
                    addTreeNode(cNode, tNode);
                }

                //treeViewYouDao.ExpandAll();

            }
            catch (XmlException xExc) //Exception is thrown is there is an error in the Xml
            {
                MessageBox.Show(xExc.Message);
            }
            catch (Exception ex) //General exception
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default; //Change the cursor back
            }
        }

         //This function is called recursively until all nodes are loaded
		private void addTreeNode(XmlNode xmlNode, TreeNode treeNode)
		{
			XmlNode xNode;
			TreeNode tNode;
			XmlNodeList xNodeList;

			if (xmlNode.HasChildNodes) //The current node has children
			{
				xNodeList = xmlNode.ChildNodes;

				for(int x=0; x<=xNodeList.Count-1; x++) //Loop through the child nodes
				{
					xNode = xmlNode.ChildNodes[x];

                    TreeNode tempNode = new TreeNode();
                    tempNode.ImageIndex = 0;
                    tempNode.SelectedImageIndex = 0;
                    tempNode.Text = xNode.Attributes["title"].Value.ToString();
                    tempNode.Name = xNode.Attributes["id"].Value;
                    if (xNode.Name == "note")
                    {
                        treeTagNote tNoteTag = new treeTagNote();
                        tNoteTag.path = xNode.Attributes["path"].Value;
                        tNoteTag.createtime = xNode.Attributes["createtime"].Value;
                        tNoteTag.updatetime = xNode.Attributes["updatetime"].Value;
                        tNoteTag.Language = xNode.Attributes["Language"].Value;
                        tempNode.ImageIndex = 1;
                        tempNode.SelectedImageIndex = 1;
                        tempNode.Tag = tNoteTag;
                    }
                    else if (xNode.Name == "book")
                    {
                        treeTagBook tBookTag = new treeTagBook();
                        tBookTag.Language = xNode.Attributes["Language"].Value;
                        tempNode.Tag = tBookTag;
                    }

					treeNode.Nodes.Add(tempNode);
					tNode = treeNode.Nodes[x];
					addTreeNode(xNode, tNode);
				}
			}
		}

		#endregion


        //双击打开文章
        private void treeViewYouDao_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeViewYouDao.SelectedNode == null)
                return;

            int iType = treeViewYouDao.SelectedNode.ImageIndex;
            if (iType == 0)
            {
                //双击目录
            }
            else
            {
                //双击文章，如果已经打开，则定位，否则新窗口打开
                string sNodeId = ((treeTagNote)treeViewYouDao.SelectedNode.Tag).path;
                string sLang = ((treeTagNote)treeViewYouDao.SelectedNode.Tag).Language;
                formParent.openNewYouDao(sNodeId, treeViewYouDao.SelectedNode.Text);

                ///打开后设置语言
                string Language = PubFunc.Synid2LanguageSetLang(PubFunc.Language2Synid(sLang));
                formParent.SetLanguage(Language);
                
            }
        }

        //新建目录
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //获取选中节点
            TreeNode SeleNode = treeViewYouDao.SelectedNode;
            string isCreateRoot = "False";
            string ParLang = "TEXT";
            
            //如果没有选中节点,则新建顶层目录
            if (SeleNode == null || treeViewYouDao.Nodes.Count == 0)
            {
                isCreateRoot = "True";
            }
            else
            {
                if (SeleNode.ImageIndex == 0)
                {
                    ParLang = ((treeTagBook)SeleNode.Tag).Language;
                }
                else if (SeleNode.ImageIndex == 1)
                {
                    ParLang = ((treeTagNote)SeleNode.Tag).Language;
                }
            }

            ProperDialog propDia = new ProperDialog("0", "", ParLang);
            DialogResult dr = propDia.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string Title = propDia.ReturnVal[0];
                string Language = propDia.ReturnVal[1];
                string IsOnRoot = propDia.ReturnVal[2];
                string sGUID = System.Guid.NewGuid().ToString();

                //新建根级目录
                if (IsOnRoot == "True"||isCreateRoot=="True")
                {
                    //插入树节点
                    TreeNode InsertNodeDir = new TreeNode(Title);
                    InsertNodeDir.Name = sGUID;
                    InsertNodeDir.ImageIndex = 0;
                    InsertNodeDir.SelectedImageIndex = 0;

                    treeTagBook tb = new treeTagBook();
                    tb.Language = Language;
                    InsertNodeDir.Tag = tb;

                    treeViewYouDao.Nodes.Insert(treeViewYouDao.Nodes.Count, InsertNodeDir);

                    //更新本地XML文档
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load("TreeNodeLocal.xml");
                    XmlNode xseleNode = xDoc.DocumentElement;

                    XmlElement appEle = xDoc.CreateElement("book");
                    appEle.SetAttribute("id", sGUID);
                    appEle.SetAttribute("title", Title);
                    appEle.SetAttribute("Language", Language);

                    xseleNode.AppendChild(appEle);
                    xDoc.Save("TreeNodeLocal.xml");

                    //同步到云端
                    XMLAPI.XML2Yun();  

                }

                else if (SeleNode != null)
                {
                    if (SeleNode.ImageIndex == 1 && IsOnRoot == "False")
                    {
                        MessageBox.Show("不能在文章下新增节点！");
                        return;
                    }
                        

                    //插入树节点
                    TreeNode InsertNodeDir = new TreeNode(Title);
                    InsertNodeDir.ImageIndex = 0;
                    InsertNodeDir.SelectedImageIndex = 0;

                    treeTagBook tb = new treeTagBook();
                    tb.Language = Language;
                    InsertNodeDir.Tag = tb;

                    SeleNode.Nodes.Insert(SeleNode.Nodes.Count, InsertNodeDir);


                    //更新本地XML文档
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load("TreeNodeLocal.xml");
                    XmlNode xseleNode = xDoc.SelectSingleNode("//book[@id='" + SeleNode.Name + "']");

                    XmlElement appEle = xDoc.CreateElement("book");
                    appEle.SetAttribute("id", sGUID);
                    appEle.SetAttribute("title", Title);
                    appEle.SetAttribute("Language", Language);
                    
                    xseleNode.AppendChild(appEle);
                    xDoc.Save("TreeNodeLocal.xml");

                    //同步到云端
                    XMLAPI.XML2Yun();
                }
            }
        }

        //右键菜单
        private void treeViewYouDao_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)//判断你点的是不是右键
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeViewYouDao.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    switch (CurrentNode.SelectedImageIndex.ToString())//根据不同节点显示不同的右键菜单
                    {
                        case "0"://目录
                            CurrentNode.ContextMenuStrip = contextMenuStripYDdir;
                            break;
                        default:
                            CurrentNode.ContextMenuStrip = contextMenuStripYDtxt;
                            break;
                    }
                    treeViewYouDao.SelectedNode = CurrentNode;//选中这个节点
                }

                else
                {
                    //右击空白区域
                    treeViewYouDao.ContextMenuStrip = contextMenuStripYDblank;
                }
            }
        }


        //新建文章
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //获取选中节点
            TreeNode SeleNode = treeViewYouDao.SelectedNode;
            string isHaveNodes = "True";
            string ParLang = "TEXT";
            //如果没有选中节点,则新建顶层目录
            if (SeleNode == null || treeViewYouDao.Nodes.Count == 0)
            {
                isHaveNodes = "False";
            }
            else {
                if (SeleNode.ImageIndex == 0)
                {
                    ParLang = ((treeTagBook)SeleNode.Tag).Language;
                }
                else if (SeleNode.ImageIndex == 1)
                {
                    ParLang = ((treeTagNote)SeleNode.Tag).Language;
                }
            }
            
            ProperDialog propDia = new ProperDialog("1", "", ParLang);
            DialogResult dr = propDia.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string Title = propDia.ReturnVal[0];
                string Language = propDia.ReturnVal[1];
                string IsOnRoot = propDia.ReturnVal[2];
                string SynId = PubFunc.Language2Synid(Language);

                string sGUID = System.Guid.NewGuid().ToString();

                //云端创建笔记，返回路径
                string Path = NoteAPI.CreateNote(Title);

                //无节点，直接顶层创建
                if (isHaveNodes == "False")
                {
                    //插入树节点
                    TreeNode InsertNodeNote = new TreeNode(Title);
                    InsertNodeNote.Name = sGUID;
                    InsertNodeNote.ImageIndex = 1;
                    InsertNodeNote.SelectedImageIndex = 1;

                    treeTagNote tag = new treeTagNote();
                    tag.path = Path;
                    tag.createtime = PubFunc.time2TotalSeconds().ToString();
                    tag.updatetime = PubFunc.time2TotalSeconds().ToString();
                    tag.Language = Language;
                    tag.isMark = "0";

                    InsertNodeNote.Tag = tag;
                    treeViewYouDao.Nodes.Insert(treeViewYouDao.Nodes.Count, InsertNodeNote);

                    //更新本地XML文档
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load("TreeNodeLocal.xml");
                    XmlNode xseleNode = xDoc.DocumentElement;

                    XmlElement appEle = xDoc.CreateElement("note");
                    appEle.SetAttribute("id", sGUID);
                    appEle.SetAttribute("title", Title);
                    appEle.SetAttribute("path", Path);
                    appEle.SetAttribute("createtime", tag.createtime);
                    appEle.SetAttribute("updatetime", tag.updatetime);
                    appEle.SetAttribute("Language", Language);
                    appEle.SetAttribute("isMark", "0");

                    xseleNode.AppendChild(appEle);
                    xDoc.Save("TreeNodeLocal.xml");

                    //同步到云端
                    XMLAPI.XML2Yun();

                    //新窗口打开编辑界面
                    formParent.openNewYouDao(Path, Title);

                    //打开后设置语言
                    Language = PubFunc.Synid2LanguageSetLang(SynId);
                    formParent.SetLanguage(Language);  
                }

                else if (SeleNode != null)
                {
                    if (Path != "")
                    {
                        //顶层创建文章
                        if (IsOnRoot == "True")
                        {
                            //插入树节点
                            TreeNode InsertNodeNote = new TreeNode(Title);
                            InsertNodeNote.Name = sGUID;
                            InsertNodeNote.ImageIndex = 1;
                            InsertNodeNote.SelectedImageIndex = 1;

                            treeTagNote tag = new treeTagNote();
                            tag.path = Path;
                            tag.createtime = PubFunc.time2TotalSeconds().ToString();
                            tag.updatetime = PubFunc.time2TotalSeconds().ToString();
                            tag.Language = Language;
                            tag.isMark = "0";

                            InsertNodeNote.Tag = tag;
                            treeViewYouDao.Nodes.Insert(treeViewYouDao.Nodes.Count, InsertNodeNote);

                            //更新本地XML文档
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load("TreeNodeLocal.xml");
                            XmlNode xseleNode = xDoc.DocumentElement;

                            XmlElement appEle = xDoc.CreateElement("note");
                            appEle.SetAttribute("id", sGUID);
                            appEle.SetAttribute("title", Title);
                            appEle.SetAttribute("path", Path);
                            appEle.SetAttribute("createtime", tag.createtime);
                            appEle.SetAttribute("updatetime", tag.updatetime);
                            appEle.SetAttribute("Language", Language);
                            appEle.SetAttribute("isMark", "0");

                            xseleNode.AppendChild(appEle);
                            xDoc.Save("TreeNodeLocal.xml");

                            //同步到云端
                            XMLAPI.XML2Yun();

                            //新窗口打开编辑界面
                            formParent.openNewYouDao(Path,Title);

                            //打开后设置语言
                            Language = PubFunc.Synid2LanguageSetLang(SynId);
                            formParent.SetLanguage(Language);   
                        }

                        else
                        {
                            if (SeleNode.ImageIndex == 1 && IsOnRoot == "False")
                            {
                                MessageBox.Show("不能在文章下新增节点！");
                                return;
                            }
                            //插入树节点
                            TreeNode InsertNodeNote = new TreeNode(Title);

                            treeTagNote tag = new treeTagNote();
                            tag.path = Path;
                            tag.createtime = PubFunc.time2TotalSeconds().ToString();
                            tag.updatetime = PubFunc.time2TotalSeconds().ToString();
                            tag.Language = Language;
                            tag.isMark = "0";

                            InsertNodeNote.ImageIndex = 1;
                            InsertNodeNote.SelectedImageIndex = 1;
                            InsertNodeNote.Tag = tag;

                            SeleNode.Nodes.Insert(SeleNode.Nodes.Count, InsertNodeNote);


                            //更新本地XML文档
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load("TreeNodeLocal.xml");
                            XmlNode xseleNode = xDoc.SelectSingleNode("//book[@id='" + SeleNode.Name + "']");

                            XmlElement appEle = xDoc.CreateElement("note");
                            appEle.SetAttribute("id", sGUID);
                            appEle.SetAttribute("title", Title);
                            appEle.SetAttribute("path", Path);
                            appEle.SetAttribute("createtime", tag.createtime);
                            appEle.SetAttribute("updatetime", tag.updatetime);
                            appEle.SetAttribute("Language", Language);
                            appEle.SetAttribute("isMark", "0");

                            xseleNode.AppendChild(appEle);
                            xDoc.Save("TreeNodeLocal.xml");

                            //同步到云端
                            XMLAPI.XML2Yun();

                            //新窗口打开编辑界面
                            formParent.openNewYouDao(Path,Title);

                            //打开后设置语言
                            Language = PubFunc.Synid2LanguageSetLang(SynId);
                            formParent.SetLanguage(Language);

                        }
                    }

                }
            }
        }
        
        //删除目录或者文章
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

            //避免保存的提示
            Attachment.isDeleteClose = "1";
            //获取选中节点
            TreeNode SeleNode = treeViewYouDao.SelectedNode;
            if (SeleNode == null)
                return;

            //删除前确认
            if (MessageBox.Show("当前节点及其所有子节点都会被删除，继续？", "提示", MessageBoxButtons.YesNo) == DialogResult.No)  
            {
                return;
            } 
            //先查找到所选节点下面所有的文章进行删除
            //删除云数据
            DelNodeData(SeleNode.Name);

            //移除树节点
            treeViewYouDao.Nodes.Remove(SeleNode);

            Attachment.isDeleteClose = "0";
 
        }

        //删除有道云数据，同时关闭已打开的文章,再更新本地XML并同步到云
        public void DelNodeData(string id)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("TreeNodeLocal.xml");
            XmlNode seleNode = doc.SelectSingleNode("//node()[@id='" + id + "']");

            if (seleNode.Name == "note")
            {
                //选中的是文章
                string path = seleNode.Attributes["path"].Value;
                NoteAPI.DeleteNote(path);
            }
            else
            {
                XmlNodeList xlist = seleNode.SelectNodes("//node()[@id='" + id + "']//note");

                foreach (XmlNode xnode in xlist)
                {
                    string path = xnode.Attributes["path"].Value;
                    //删除笔记
                    NoteAPI.DeleteNote(path);
                }
            }

            //移除XML节点，更新XML到云

            seleNode.ParentNode.RemoveChild(seleNode);
            doc.Save("TreeNodeLocal.xml");
            XMLAPI.XML2Yun();

        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            toolStripMenuItem3_Click(sender, e);
        }


        #region 上下移动操作
        //上移
        public void setNodeUp()
        {
            //树操作
            SetTreeNodeUp(this.treeViewYouDao.SelectedNode);

        }

        private void SetTreeNodeUp(System.Windows.Forms.TreeNode node)
        {
            if ((node == null) || (node.PrevNode) == null) return;
            System.Windows.Forms.TreeNode newNode = (System.Windows.Forms.TreeNode)node.Clone();

            //要交换次序的节点
            string NodeId1 = node.Name.ToString();
            string NodeId2 = node.PrevNode.Name.ToString();

            if (node.Parent != null)
                node.Parent.Nodes.Insert(node.PrevNode.Index, newNode);
            else
                node.TreeView.Nodes.Insert(node.PrevNode.Index, newNode);

            this.treeViewYouDao.Nodes.Remove(node);
            this.treeViewYouDao.SelectedNode = newNode;

            treeViewYouDao.Focus();

            //xml移动
            xmlNodeMove(NodeId2, NodeId1);

            //xml同步
            XMLAPI.XML2Yun();
        }


        //下移
        public void setNodeDown()
        {
            //树操作
            SetTreeNodeDown(this.treeViewYouDao.SelectedNode);

        }

        private void SetTreeNodeDown(System.Windows.Forms.TreeNode node)
        {
            if ((node == null) || (node.NextNode) == null) return;
            System.Windows.Forms.TreeNode newNode = (System.Windows.Forms.TreeNode)node.Clone();

            //要交换次序的节点
            string NodeId1 = node.Name.ToString();
            string NodeId2 = node.NextNode.Name.ToString();

            if (node.Parent != null)
                node.Parent.Nodes.Insert(node.NextNode.Index + 1, newNode);
            else
                node.TreeView.Nodes.Insert(node.NextNode.Index + 1, newNode);

            this.treeViewYouDao.Nodes.Remove(node);
            this.treeViewYouDao.SelectedNode = newNode;

            treeViewYouDao.Focus();

            //xml移动
            xmlNodeMove(NodeId1, NodeId2);

            //xml同步
            XMLAPI.XML2Yun();
        }


        /// <summary>
        /// 交换xml节点的顺序
        /// </summary>
        /// <param name="id1">前一个节点id</param>
        /// <param name="id2">后一个节点id</param>
        private void xmlNodeMove(string id1, string id2)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("TreeNodeLocal.xml");
            XmlNode preNode = doc.SelectSingleNode("//node()[@id='" + id1 + "']");
            XmlNode parentNode = preNode.ParentNode;
            XmlNode nexNode = doc.SelectSingleNode("//node()[@id='" + id2 + "']");

            parentNode.InsertAfter(preNode, nexNode);
            doc.Save("TreeNodeLocal.xml");
        }

        #endregion

        
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1_Click(sender, e);
        }

        //重命名以及语言
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            //获取选中节点
            TreeNode SeleNode = this.treeViewYouDao.SelectedNode;
            if (SeleNode == null)
                return;
            string ParLang, Type, DiaType;
            if (SeleNode.ImageIndex == 0)
            {
                //目录
                ParLang = ((treeTagBook)SeleNode.Tag).Language;
                DiaType = "2";
            }
            else
            {
                ParLang = ((treeTagNote)SeleNode.Tag).Language;
                DiaType = "3";
            }

            ProperDialog propDia = new ProperDialog(DiaType, SeleNode.Text, ParLang);
            DialogResult dr = propDia.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string Title = propDia.ReturnVal[0];
                string Language = propDia.ReturnVal[1];
                string SynId = PubFunc.Language2Synid(Language);

                if (SeleNode != null)
                {
                    //更新标题和语言
                    XmlDocument doc = new XmlDocument();
                    doc.Load("TreeNodeLocal.xml");
                    XmlNode preNode = doc.SelectSingleNode("//node()[@id='" + SeleNode.Name + "']");
                    preNode.Attributes["title"].Value = Title;
                    preNode.Attributes["Language"].Value = Language;
                    doc.Save("TreeNodeLocal.xml");
                    XMLAPI.XML2Yun();
                    //更新节点信息
                    SeleNode.Text = Title;
                    if (SeleNode.ImageIndex == 0)
                    {
                        treeTagBook tb = new treeTagBook();
                        tb.Language = Language;
                        SeleNode.Tag = tb;
                    }
                    else {
                        treeTagNote tag = new treeTagNote();
                        tag.path = ((treeTagNote)SeleNode.Tag).path;
                        tag.createtime = ((treeTagNote)SeleNode.Tag).createtime;
                        tag.updatetime = PubFunc.time2TotalSeconds().ToString();
                        tag.Language = Language;
                        tag.isMark = "0";
                        SeleNode.Tag = tag;
                    }
                    //打开后设置语言
                    Language = PubFunc.Synid2LanguageSetLang(SynId);

                }
            }
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            toolStripMenuItem4_Click(sender, e);
        }

        //加为书签
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            //获取选中节点
            TreeNode SeleNode = treeViewYouDao.SelectedNode;
            if (SeleNode == null)
                return;
            XmlDocument doc = new XmlDocument();
            doc.Load("TreeNodeLocal.xml");
            XmlNode preNode = doc.SelectSingleNode("//node()[@id='" + SeleNode.Name + "']");
            preNode.Attributes["isMark"].Value = "1";
            doc.Save("TreeNodeLocal.xml");
            XMLAPI.XML2Yun();
        }

        private void YouDaoTree_Activated(object sender, EventArgs e)
        {
            
        }

        private void YouDaoTree_Deactivate(object sender, EventArgs e)
        {
            
        }

        //供主窗口调用
        public void NewDoc()
        {
            toolStripMenuItem2_Click(null, null);
        }

        //供主窗口调用
        public void NewDir()
        {
            toolStripMenuItem1_Click(null, null);
        }

        //供主窗口调用
        public void DelNode()
        {
            toolStripMenuItem3_Click(null, null);
        }

        //供主窗口调用
        public void ShowProp()
        {
            toolStripMenuItem4_Click(null, null);
        }

        private void treeViewYouDao_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode seleNode = treeViewYouDao.SelectedNode;
                if (seleNode == null)
                    return;
                if (seleNode.ImageIndex == 0)
                    return;
                string path = seleNode.FullPath;
                int TotalSeconds = Convert.ToInt32(((treeTagNote)seleNode.Tag).createtime);
                DateTime cTime = PubFunc.seconds2Time(TotalSeconds);
                string createTime = "创建： " + cTime.ToString();
                formParent.showFullPathTime(path, createTime);
            }
            catch (Exception wx)
            {
                throw wx;
            }
        }

    }
}
