using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace wpfInBefore404
{

  public class HtmlParser : IDisposable
  {
    private string htmlData;
    private static string RegExprFrame = @"(?<=frame\s+src\=[\x27\x22])(?<1>[^\x27\x22]*)(?=[\x27\x22])";
    private static string RegExprHREF = @"(?<=a\s+([^>]+\s+)?href\=[\x27\x22])(?<1>[^\x27\x22]*)(?=[\x27\x22])";
    private static string RegExprIFrame = @"(?<=iframe\s+src\=[\x27\x22])(?<1>[^\x27\x22]*)(?=[\x27\x22])";
    private static string RegExprImg = "/\\< *[img][^\\>]*[src] *= *[\\\"\\']{0,1}([^\\\"\\'\\ >]*)/i";
    private static Regex RegExFindFrame = new Regex(RegExprFrame, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex RegExFindHref = new Regex(RegExprHREF, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex RegExFindIFrame = new Regex(RegExprIFrame, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex RegExFindImg = new Regex(RegExprImg, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex[] regs = new Regex[] { RegExFindHref, RegExFindImg, RegExFindIFrame, RegExFindFrame };


    public HtmlParser(string argHTMLString)
    {
      this.htmlData = argHTMLString;
    }

    private static Uri ConvertToAbsoluteUrl(string url, string baseUrl)
    {
      if ((url.IndexOf(Uri.SchemeDelimiter) < 0) && (baseUrl != null)) {
        try {
          return new Uri(new Uri(baseUrl), url);
        } catch {
          return null;
        }
      }
      try {
        return new Uri(url);
      } catch (Exception) {
        if (baseUrl != null) {
          try {
            return new Uri(new Uri(baseUrl), url);
          } catch (Exception) {
            return null;
          }
        }
        return null;
      }
    }

    public void Dispose()
    {
      this.htmlData = null;
    }

    private static IEnumerable<Uri> GetEnumerator(Regex regExpr, string baseUrl, string html)
    {
      for (Match iteratorVariable0 = regExpr.Match(html); iteratorVariable0.Success; iteratorVariable0 = iteratorVariable0.NextMatch()) {
        string iteratorVariable1 = iteratorVariable0.Groups[1].ToString();
        if (((!string.IsNullOrEmpty(iteratorVariable1) && !iteratorVariable1.StartsWith("#")) && (!iteratorVariable1.StartsWith("mailto:") && !iteratorVariable1.StartsWith("javascript:"))) && !iteratorVariable1.EndsWith("/")) {
          iteratorVariable1 = HttpUtility.HtmlDecode(iteratorVariable1);
          Uri iteratorVariable2 = null;
          try {
            iteratorVariable2 = ConvertToAbsoluteUrl(iteratorVariable1, baseUrl);
          } catch (Exception) {
          }
          if (iteratorVariable2 != null) {
            yield return iteratorVariable2;
          }
        }
      }
    }

    public IEnumerable<Uri> GetFrames(string baseUri)
    {
      return GetEnumerator(RegExFindFrame, baseUri, this.htmlData);
    }

    public IEnumerable<Uri> GetHrefs(string baseUri)
    {
      return GetEnumerator(RegExFindHref, baseUri, this.htmlData);
    }

    public IEnumerable<Uri> GetIFrames(string baseUri)
    {
      return GetEnumerator(RegExFindIFrame, baseUri, this.htmlData);
    }

    public IEnumerable<Uri> GetImages(string baseUri)
    {
      return GetEnumerator(RegExFindImg, baseUri, this.htmlData);
    }

    public IEnumerable<Uri> GetResources(UrlType urlType, string baseUri)
    {
      return GetEnumerator(regs[(int)urlType], baseUri, this.htmlData);
    }

  }
}

