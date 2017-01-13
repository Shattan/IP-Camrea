//----------------------------------------------------------------------------
//  This file is automatically generated, do not modify.      
//----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;

namespace Emgu.CV.ML
{
   public static partial class MlInvoke
   {

     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern int cveBoostGetMaxCategories(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetMaxCategories(
        IntPtr obj,  
        int val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern int cveBoostGetMaxDepth(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetMaxDepth(
        IntPtr obj,  
        int val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern int cveBoostGetMinSampleCount(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetMinSampleCount(
        IntPtr obj,  
        int val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern int cveBoostGetCVFolds(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetCVFolds(
        IntPtr obj,  
        int val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     [return: MarshalAs(CvInvoke.BoolMarshalType)]
     internal static extern bool cveBoostGetUseSurrogates(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetUseSurrogates(
        IntPtr obj, 
        [MarshalAs(CvInvoke.BoolMarshalType)] 
        bool val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     [return: MarshalAs(CvInvoke.BoolMarshalType)]
     internal static extern bool cveBoostGetUse1SERule(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetUse1SERule(
        IntPtr obj, 
        [MarshalAs(CvInvoke.BoolMarshalType)] 
        bool val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     [return: MarshalAs(CvInvoke.BoolMarshalType)]
     internal static extern bool cveBoostGetTruncatePrunedTree(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetTruncatePrunedTree(
        IntPtr obj, 
        [MarshalAs(CvInvoke.BoolMarshalType)] 
        bool val);
     
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)] 
     internal static extern float cveBoostGetRegressionAccuracy(IntPtr obj);
     [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
     internal static extern void cveBoostSetRegressionAccuracy(
        IntPtr obj,  
        float val);
     
   }

   public partial class Boost
   {

     /// <summary>
     /// Cluster possible values of a categorical variable into K less than or equals maxCategories clusters to find a suboptimal split
     /// </summary>
     public int MaxCategories
     {
        get { return MlInvoke.cveBoostGetMaxCategories(_ptr); } 
        set { MlInvoke.cveBoostSetMaxCategories(_ptr, value); }
     }
     
     /// <summary>
     /// The maximum possible depth of the tree
     /// </summary>
     public int MaxDepth
     {
        get { return MlInvoke.cveBoostGetMaxDepth(_ptr); } 
        set { MlInvoke.cveBoostSetMaxDepth(_ptr, value); }
     }
     
     /// <summary>
     /// If the number of samples in a node is less than this parameter then the node will not be split
     /// </summary>
     public int MinSampleCount
     {
        get { return MlInvoke.cveBoostGetMinSampleCount(_ptr); } 
        set { MlInvoke.cveBoostSetMinSampleCount(_ptr, value); }
     }
     
     /// <summary>
     /// If CVFolds greater than 1 then algorithms prunes the built decision tree using K-fold
     /// </summary>
     public int CVFolds
     {
        get { return MlInvoke.cveBoostGetCVFolds(_ptr); } 
        set { MlInvoke.cveBoostSetCVFolds(_ptr, value); }
     }
     
     /// <summary>
     /// If true then surrogate splits will be built
     /// </summary>
     public bool UseSurrogates
     {
        get { return MlInvoke.cveBoostGetUseSurrogates(_ptr); } 
        set { MlInvoke.cveBoostSetUseSurrogates(_ptr, value); }
     }
     
     /// <summary>
     /// If true then a pruning will be harsher
     /// </summary>
     public bool Use1SERule
     {
        get { return MlInvoke.cveBoostGetUse1SERule(_ptr); } 
        set { MlInvoke.cveBoostSetUse1SERule(_ptr, value); }
     }
     
     /// <summary>
     /// If true then pruned branches are physically removed from the tree
     /// </summary>
     public bool TruncatePrunedTree
     {
        get { return MlInvoke.cveBoostGetTruncatePrunedTree(_ptr); } 
        set { MlInvoke.cveBoostSetTruncatePrunedTree(_ptr, value); }
     }
     
     /// <summary>
     /// Termination criteria for regression trees
     /// </summary>
     public float RegressionAccuracy
     {
        get { return MlInvoke.cveBoostGetRegressionAccuracy(_ptr); } 
        set { MlInvoke.cveBoostSetRegressionAccuracy(_ptr, value); }
     }
     
   }
}