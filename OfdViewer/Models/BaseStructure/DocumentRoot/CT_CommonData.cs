﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using OFDViewer.Models.BaseStructure.Pages;
using OFDViewer.Models.BaseType;
using OFDViewer.Utils;

namespace OFDViewer.Models.BaseStructure.DocumentRoot
{
    /// <summary>
    /// 公共数据复杂类型 图 6
    /// </summary>
    public class CT_CommonData
    {
        /// <summary>
        /// 当前文档中所有对象使用标识的最大值,初始值为0。MaxUnitID
        /// 主要用于文档编辑,在向文档中新增加一个对象时,需要分配一个
        /// 新的标识, 新标识取值宜为 MaxUnitID + 1,同时需要修改此 Max-UnitID 值
        /// 必选
        /// </summary>
        [XmlElement("MaxUnitID")]
        [XmlRequired(ErrorMsg = "标识的最大值为必选属性，不能为空")]
        public ST_ID MaxUnitID { get; set; }

        /// <summary>
        /// 指定该文档页面区域的默认大小和位置 
        /// 必选
        /// </summary>
        [XmlElement("PageArea")]
        [XmlRequired(ErrorMsg = "页面区域为必选属性，不能为空")]
        public CT_PageArea PageArea { get; set; }

        /// <summary>
        /// 公共资源序列,每个节点指向 OFD 包内的一个资源描述文档,资源
        /// 部分的描述见7.9,字型和颜色空间等宜在公共资源文件中描述
        /// 可选（0..∞）
        /// </summary>
        [XmlElement("PublicRes")]
        public List<ST_Loc> PublicRes { get; set; }


        /// <summary>
        /// 文档资源序列,每个节点指向 OFD 包内的一个资源描述文档,资源
        /// 部分的描述见7.9,绘制参数、多媒体和矢量图像等宜在文档资源文件中描述
        /// 可选（0..∞）
        /// </summary>
        [XmlElement("DocumentRes")]
        public List<ST_Loc> DocumentRes { get; set; }


        /// <summary>
        /// 模板页序列,为一系列模板页的集合,模板页内容结构和普通页相同,描述见7.7
        /// 可选（0..∞）
        /// </summary>
        [XmlElement("TemplatePage")]
        public List<CT_TemplatePage> TemplatePage { get; set; }

        /// <summary>
        /// 引用在资源文件中定义的颜色空间标识,有关颜色空间的描述见8.3.1。如果此项不存在,采用 RGB 作为默认颜色空间
        /// 可选
        /// </summary>
        [XmlElement("DefaultCS")]
        public ST_RefID DefaultCS { get; set; }

        //无参构造函数，必选属性初始化
        public CT_CommonData()
        {
            MaxUnitID = ST_ID.Invalid;
            PageArea = new CT_PageArea();
        }
    }
}