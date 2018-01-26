using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Threading;
using log4net.Config;
using System.Globalization;
using System.IO;
using log4net;
using System.Data.Common;
using System.Xml;
namespace sfwctrlTache
{
    /// <summary>
    /// Description résumée de Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    
    
    
    public class Service1 : System.Web.Services.WebService
    {
        private static string password = "safeware";
        public static DbConnection getCnx()
        {
            string strInvariant = "";
            string _user, _pwd, _server, _type, _connectionstring;
            string _xmlUser, _xmlPwd, _xmlServer, _xmlType, _xmlConnectionstring;
            DbConnection cnx;
            DbProviderFactory factory;
            string type = "";
            XmlDocument xml = new XmlDocument();
            XmlDocument xmlWebConfig = new XmlDocument();
            //string _fichierZcl = sfwctrTache.Properties.Settings.Default.zclParamRep.ToString() + @"\WSZCL.xml";
            string chemin_webconfig = @"C:\inetpub\ws_unis\web.config";
            bool _erreurZcl = false;
            string _fichierZcl = "";
           
            try
            {
                xmlWebConfig.Load(chemin_webconfig);

            }
            catch (Exception ex)
            {

                _log.Error("Fichier " + chemin_webconfig + "non trouvé" + ex.Message);
                return null;
            
            }
            XmlNode _node = xmlWebConfig.SelectSingleNode("/configuration/applicationSettings/SafeWare.Properties.Settings");
            foreach (XmlNode _xmlNode in _node.ChildNodes)
            {
                if (_xmlNode.Name.Equals("Settings") && _xmlNode.Attributes["name"].Equals("zclParamRep")) {

                _fichierZcl=_xmlNode.InnerText;
                break;
                }
            }            
            
            try
            {
                xml.Load(_fichierZcl);

            }
            catch (Exception ex)
            {

                _log.Error("Fichier " + _fichierZcl + "non trouvé" + ex.Message);               
                return null;
            }

            if (!_erreurZcl)
            {

                _xmlUser = (xml.GetElementsByTagName("USER").Item(0) == null) ? "" : xml.GetElementsByTagName("USER").Item(0).InnerText;
                _xmlPwd = (xml.GetElementsByTagName("PASSWORD").Item(0) == null) ? "" : xml.GetElementsByTagName("PASSWORD").Item(0).InnerText;
                _xmlType = (xml.GetElementsByTagName("TYPE").Item(0) == null) ? "" : xml.GetElementsByTagName("TYPE").Item(0).InnerText;
                _xmlServer = (xml.GetElementsByTagName("SERVER").Item(0) == null) ? "" : xml.GetElementsByTagName("SERVER").Item(0).InnerText;
                _xmlConnectionstring = (xml.GetElementsByTagName("CONNECTIONSTRING").Item(0) == null) ? "" : xml.GetElementsByTagName("CONNECTIONSTRING").Item(0).InnerText;

            }
            else
            {
                _xmlUser = (sfwctrlTache.Properties.Settings.Default.USER == null) ? "" : sfwctrlTache.Properties.Settings.Default.USER;
                _xmlPwd = (sfwctrlTache.Properties.Settings.Default.PASSWORD == null) ? "" : sfwctrlTache.Properties.Settings.Default.PASSWORD;
                _xmlType = (sfwctrlTache.Properties.Settings.Default.TYPE == null) ? "" : sfwctrlTache.Properties.Settings.Default.TYPE;
                _xmlServer = (sfwctrlTache.Properties.Settings.Default.SERVER == null) ? "" : sfwctrlTache.Properties.Settings.Default.SERVER;
                _xmlConnectionstring = (sfwctrlTache.Properties.Settings.Default.CONNECTIONSTRING == null) ? "" : sfwctrlTache.Properties.Settings.Default.CONNECTIONSTRING;

            }

            try
            {
                if (!_erreurZcl || (sfwctrlTache.Properties.Settings.Default.Properties["zina"] != null && !sfwctrlTache.Properties.Settings.Default.zina.Equals("false")))
                {
                    _user = Chiffrement.dechiffre(_xmlUser, password);
                    _pwd = Chiffrement.dechiffre(_xmlPwd, password);
                    _server = Chiffrement.dechiffre(_xmlServer, password);
                    _type = Chiffrement.dechiffre(_xmlType, password);
                    _connectionstring = Chiffrement.dechiffre(_xmlConnectionstring, password);

                }
                else
                {
                    _user = _xmlUser;
                    _pwd = _xmlPwd;
                    _server = _xmlServer;
                    _type = _xmlType;
                    _connectionstring = _xmlConnectionstring;

                    _log.Debug("CNX 1 : " + _xmlConnectionstring);
                }

                if (_type.ToUpper().Equals("ORACLE"))
                {
                    strInvariant = "System.Data.OracleClient";
                    _log.Debug("CNX 2 : " + _xmlConnectionstring);
                }
                else
                {
                    strInvariant = "System.Data.SqlClient";
                }

                factory = DbProviderFactories.GetFactory(strInvariant);

                cnx = factory.CreateConnection();

                if (!String.IsNullOrEmpty(_connectionstring))
                {

                    cnx.ConnectionString = _connectionstring;
                    _log.Debug("CNX 3 : " + _xmlConnectionstring);
                }
                else
                {
                    DbConnectionStringBuilder builder = factory.CreateConnectionStringBuilder();

                    builder.Add("Server", _server);
                    builder.Add("Uid", _user);
                    builder.Add("Pwd", _pwd);
                    cnx.ConnectionString = builder.ConnectionString;
                }


                cnx.Open();
                _log.Debug("Connexion OK ");
                return cnx;
            }
            catch (DbException exp)
            {
                _log.Error("Connexion échouée : " + exp.Message);
                return null;
            }

        }

        public static string getNouvelId(string nomTable)
        {
            DbConnection cnx = getCnx();
            string SQLDestServerType = (cnx.GetType().ToString().Equals("System.Data.SqlClient.SqlConnection")) ? "SQL SERVER" : "ORACLE";
            using (cnx)
            {
                DbCommand cmd = cnx.CreateCommand();
                using (cmd)
                {
                    string reqString = "";
                    switch (SQLDestServerType)
                    {
                        case "SQL SERVER":
                            reqString = "INSERT INTO SEQ_IDENTITY (LIBELLE) VALUES ('OK') ; SELECT @@IDENTITY AS ID";
                            break;
                        case "ORACLE":
                            reqString = "SELECT SEQ_:TABLE.NEXTVAL FROM DUAL";
                            break;
                        default:
                            reqString = "";
                            break;
                    }
                    reqString = reqString.Replace(":TABLE", nomTable);
                    cmd.CommandText = reqString;
                    try
                    {
                        Decimal id = Decimal.Parse(cmd.ExecuteScalar().ToString());
                        _log.Info("ID :<" + id.ToString() + ">");
                        return id.ToString();

                    }
                    catch (Exception ex)
                    {
                        _log.Error("ERREUR CREATION ID :<" + reqString + ">" + ex.Message);
                        return null;
                    }
                }
            }
        }
        public static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        [WebMethod]
        public string ctrlTache()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
                log4net.Config.XmlConfigurator.Configure();
                
                _log.Debug("APPEL AU CONTROLE DES TACHES");
                DbConnection cnx = getCnx();
                
            }
            catch (Exception ex)
            {
                _log.Error("ERREUR AU COURS DE LA TENTATIVE DE CONTROLE DES TACHES " + ex.Message);

            }

            return "ctrl Done";
        }
    }
}