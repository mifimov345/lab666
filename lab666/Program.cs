using System.Xml.Linq;
using System.Text.RegularExpressions;

///ПЕРЕДЕЛАЙТЕ ВСЕ ЦИКЛЫ НА LINQ

Num5(GetPath("test.txt"), GetPath("num5.xml"));
Num15(GetPath("num15.xml"));
Num25(GetPath("num15.xml"), GetPath("num25.xml"));
Num35(GetPath("num15.xml"), GetPath("num35.xml"));
Num45(GetPath("num15.xml"), GetPath("num45.xml"));
Num55(GetPath("num55_before.xml"), GetPath("num55_after.xml"));
Num65(GetPath("num65_before.xml"), GetPath("num65_after.xml"));
Num75(GetPath("num75_before.xml"), GetPath("num75_after.xml"));

static string GetPath(string name)
{

    string[] start = System.Reflection.Assembly.GetExecutingAssembly().Location.Split('\\');
    string path = "";
    for (int i = 0; i < 7; i++)
    {
        path += start[i];
        path += "\\";
    }
    path += $"Resources\\{name}";
    return path;
}

void Num5(string txtName, string xmlName)
{
    string[] lines = File.ReadAllLines(txtName);

    var res = new XDocument(
        new XDeclaration("1.0", "utf-8", "no")
    );

    XElement root = new XElement("root");
    res.Add(root);

    int num = 0;
    foreach (string line in lines)
    {
        var lineNode = new XElement("line");
        lineNode.SetAttributeValue("num", num);
        int wordNum = 0;
        foreach (var word in line.Split(' '))
        {
            var wordNode = new XElement("word");
            wordNode.Add(word);
            wordNode.SetAttributeValue("num", wordNum);
            lineNode.Add(wordNode);
            wordNum++;
        }
        root.Add(lineNode);
        num++;
    }

    res.Save(xmlName);
}

void Num15(string xmlName)
{
    var doc = XDocument.Load(xmlName);

    if (doc.Root == null | doc.Root.Elements() == null) return;

    var Nodes1 = doc.Root.Elements();

    string[] names = new string[Nodes1.Count()];
    int i = 0;

    int numOfAttributes = 0;
    foreach (var node1 in Nodes1)
    {
        foreach (var node2 in node1.Elements())
        {
            if (node2.Attributes().Count() >= 2)
            {
                numOfAttributes++;
            }
        }
        if (numOfAttributes > 0)
        {
            names[i] = node1.Name.LocalName + " " + numOfAttributes;
            i++;
        }
        numOfAttributes = 0;
    }

    foreach (string name in names.OrderBy(w => w))
    {
        Console.WriteLine(name);
    }
}

void Num25(string xmlName, string resName)
{
    var doc = XDocument.Load(xmlName);

    var Nodes1 = doc.Root.Elements();

    foreach (var node1 in Nodes1)
    {
        if (node1.Attributes().Count() > 1)
            node1.Attributes().Remove();

        foreach (var node2 in node1.Elements())
            if (node2.Attributes().Count() > 1)
                node2.Attributes().Remove();
    }

    doc.Save(resName);
}

void Num35(string xmlName, string resName)
{
    var doc = XDocument.Load(xmlName);

    var Nodes1 = doc.Root.Elements();

    foreach (var node1 in Nodes1)
    {
        foreach (var node2 in node1.Elements())
            if (node2.Elements() != null)
                node2.SetAttributeValue("child-count", node2.Elements().Count());
            else
                node2.SetAttributeValue("child-count", 0);
    }

    doc.Save(resName);
}

void Num45(string xmlName, string resName)
{
    var doc = XDocument.Load(xmlName);

    foreach (var node in doc.Descendants())
    {
        if (node.Attributes().Any())
        {
            int count = node.Attributes().Count();
            foreach (var child in node.Descendants())
                count += child.Attributes().Count();
            if (count % 2 == 0)
                node.AddFirst(new XElement("odd-attr-count", false));
            else
                node.AddFirst(new XElement("odd-attr-count", true));
        }
    }

    doc.Save(resName);
}

void Num55(string xmlName, string resName)
{
    var doc = XDocument.Load(xmlName);

    foreach (var node in doc.Root.Elements().Elements())
        node.Name = node.Name.LocalName;

    doc.Save(resName);
}

void Num65(string xmlName, string resName)
{
    var doc = XDocument.Load(xmlName);

    var res = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
    res.Add(new XElement("root"));

    //достаём года и создаём узлы
    foreach (var node1 in doc.Root.Elements())
        foreach (var node2 in node1.Elements())
            res.Root.Add(new XElement("year", new XAttribute("value", DateTime.Parse(node2.Element("date").Value).Year)));
    //группируем года
    res.Root.ReplaceNodes(res.Root.Elements().GroupBy(x => x.Attribute("value").Value).Select(s => new XElement("year", new XAttribute("value", s.Attributes().First().Value))));

    foreach (var node1 in doc.Root.Elements())
        foreach (var nodeRes1 in res.Root.Elements())
            foreach (var node2 in node1.Elements())
                //если дата года совпадает добавляем клиента
                if (DateTime.Parse(node2.Element("date").Value).Year.ToString() == nodeRes1.Attribute("value").Value)
                    //регулярное выражение, достающие цифры
                    nodeRes1.Add(new XElement("total-time", new XAttribute("id", node1.Name.ToString().Split("id")[1]), Convert.ToInt32(Regex.Matches(node2.Element("time").Value, @"\d+")[0].ToString()) * 60 + Convert.ToInt32(Regex.Matches(node2.Element("time").Value, @"\d+")[1].ToString())));

    //сортируем
    res.Root.ReplaceNodes(res.Root.Elements().OrderBy(x => x.Attribute("value").Value));
    foreach (var node in res.Root.Elements())
        node.ReplaceNodes(node.Elements().OrderBy(x => x.Attribute("id").Value));

    res.Save(resName);
}

void Num75(string xmlName, string resName)
{
    var doc = XDocument.Load(xmlName);

    var res = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
    res.Add(new XElement("root"));

    res.Root.ReplaceNodes(doc.Root.Elements()
       .GroupBy(s => s.Name.LocalName.Split('_')[1]) //группируем по улицам
       .Select(s => new XElement(s.Key, s.ToList()
            .GroupBy(s => s.Element("brand").Value) //группируем по брендам
            .Select(s => new XElement("brand" + s.Key,
                new XAttribute("station-count", s.Count()), //колическо группировок по брендам - количество станций
                (s.Elements().Select(s => int.Parse(s.Value)).Sum() - int.Parse(s.Key) * s.Count()) / s.Count() //подсчёт средней цены
            )
            )
        )
       )
    );

    foreach (var node in res.Root.Elements()) //добавляем бренды, которых нет на улице
    {
        var brands = node.Elements().Select(s => s.Name).ToList();
        if (!brands.Contains("brand98"))
            node.Add(new XElement("brand98", new XAttribute("station-count", 0), 0));
        if (!brands.Contains("brand95"))
            node.Add(new XElement("brand95", new XAttribute("station-count", 0), 0));
        if (!brands.Contains("brand92"))
            node.Add(new XElement("brand92", new XAttribute("station-count", 0), 0));
    }

    //сортируем
    res.Root.ReplaceNodes(res.Root.Elements().OrderBy(x => x.Name.LocalName));
    foreach (var node in res.Root.Elements())
        node.ReplaceNodes(node.Elements().OrderByDescending(x => x.Name.LocalName.Split("brand")[1]));

    res.Save(resName);
}