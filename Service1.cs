using log4net;
using log4net.Config;
using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading;
using System.Timers;
using System.Xml;


namespace sfwServiceTache
{
    public partial class Service1 : ServiceBase
    {
        //   ws_importation.Service1 ws_imp = new ws_importation.Service1();
        System.Timers.Timer timerWsImportation = new System.Timers.Timer();
        System.Timers.Timer timerCtrlTache = new System.Timers.Timer();
        System.Timers.Timer timerOmni = new System.Timers.Timer();
        System.Timers.Timer timerDyn = new System.Timers.Timer();
        System.Timers.Timer timerRapide = new System.Timers.Timer();
        System.Timers.Timer timerMinute = new System.Timers.Timer();
        System.Timers.Timer timerGet = new System.Timers.Timer();
        int _idx = 0;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ws_importation.wsimport ws_imp;
        object userState = "";
        string retourGlb = "";
        public Service1()
        {
            InitializeComponent();

            XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            log4net.Config.XmlConfigurator.Configure();
            //doCallWsOmni();
            //doCallWsImportation();
            //doCallCheckTasks();

            //////////////timer//////////////
            int result;
            if (int.TryParse(sfwServiceTache.Properties.Settings.Default.timer_ws_importation.ToString(), out result))
            {
                timerWsImportation.Interval = result;
            }
            else
            {

                timerWsImportation.Interval = 30000;
            }

            timerWsImportation.Elapsed += new ElapsedEventHandler(ExecutionAsynch);
            timerWsImportation.Start();

            //////////////timer//////////////
            if (int.TryParse(sfwServiceTache.Properties.Settings.Default.timerOmni.ToString(), out result))
            {
                timerOmni.Interval = result;
            }
            else
            {

                timerOmni.Interval = 120000;
            }
            if (int.TryParse(sfwServiceTache.Properties.Settings.Default.timerGet.ToString(), out result))
            {
                timerGet.Interval = result;
            }
            else
            {

                timerGet.Interval = 120000;
            }

            if (int.TryParse(sfwServiceTache.Properties.Settings.Default.timerDyn.ToString(), out result))
            {
                timerDyn.Interval = result;
            }
            else
            {

                timerDyn.Interval = 120000;
            }

            timerOmni.Elapsed += new ElapsedEventHandler(ExecOmni);
            timerCtrlTache.Elapsed += new ElapsedEventHandler(ExecCheckTasks);
            timerOmni.Start();
            timerCtrlTache.Start();

            timerGet.Elapsed += new ElapsedEventHandler(ExecGetCalls);
            timerGet.Start();

            timerDyn.Elapsed += new ElapsedEventHandler(ExecDynCalls);
            timerDyn.Start();


            //////////////timer rapide 2s //////////////
            timerRapide.Interval = 2000;
            timerRapide.Elapsed += new ElapsedEventHandler(timerRapideCode);
            timerRapide.Start();
            //timerRapide.Stop();

            //////////////timer rapide 1m //////////////
            timerMinute.Interval = 60000;
            timerMinute.Elapsed += new ElapsedEventHandler(timerMinuteCode);
            timerMinute.Start();


        }

        protected override void OnStart(string[] args)
        {
            _idx++;
            try
            {
                log.Debug("Service tache Start : " + _idx.ToString());
                calculNextTick();
                doCallWsOmni();
                //doCallWsImportation();
                doCallWsImportationAsynch();
                doCallCheckTasks();

            }
            catch (Exception ex)
            {

                log.Error("APPEL AU DEMARRAGE " + ex.Message);

            }
        }

        protected override void OnStop()
        {
            try
            {

                ws_imp.CancelAsync(userState);
                //  ws_imp.TacheImportCompleted -= new ws_importation.TacheImportCompletedEventHandler(this.TacheImportCompleted);
            }
            catch (Exception ex)
            {

            }
            log.Debug("Service Tache Stop");
            timerRapide.Stop();
            timerMinute.Stop();
            timerDyn.Stop();
        }
        private void calculNextTick()
        {
            try
            {
                log.Debug("Service_CRMUSAM Start");
                TimeSpan heureTimer = new TimeSpan(sfwServiceTache.Properties.Settings.Default.heureFixe.Hours, sfwServiceTache.Properties.Settings.Default.heureFixe.Minutes, sfwServiceTache.Properties.Settings.Default.heureFixe.Seconds);
                log.Debug("heure prévue : " + heureTimer.ToString());

                TimeSpan horairePresent = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                log.Debug("horaire présent : " + horairePresent.ToString());

                TimeSpan intervall = heureTimer - horairePresent;
                log.Debug("délai attente : " + intervall.ToString());

                TimeSpan heur24 = new TimeSpan(23, 59, 00);


                if (intervall.Ticks <= 0)
                {
                    log.Debug("timerCtrlTache.interval avant : " + timerCtrlTache.Interval.ToString());
                    timerCtrlTache.Interval = ((intervall + heur24).Seconds + (intervall + heur24).Minutes * 60 + (intervall + heur24).Hours * 3600) * 1000;
                    log.Debug("timerCtrlTache.interval après : " + timerCtrlTache.Interval.ToString() + "[" + (intervall + heur24).Hours.ToString() + ":" + (intervall + heur24).Minutes.ToString() + ":" + (intervall + heur24).Seconds.ToString() + "]");
                }
                else
                {
                    log.Debug("timerCtrlTache.interval avant : " + timerCtrlTache.Interval.ToString());
                    timerCtrlTache.Interval = ((intervall).Seconds + (intervall).Minutes * 60 + (intervall).Hours * 3600) * 1000;
                    log.Debug("timerCtrlTache.interval après : " + timerCtrlTache.Interval.ToString() + "[" + (intervall).Hours.ToString() + ":" + (intervall).Minutes.ToString() + ":" + (intervall).Seconds.ToString() + "]");
                }

            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
            }
        }
        private void Execution(object source, ElapsedEventArgs e)
        {
            doCallWsImportation();
        }

        private static void doCallWsImportation()
        {
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.liste_wsimport_soap.ToString()))
            {
                string[] liste_ws_importation = sfwServiceTache.Properties.Settings.Default.liste_wsimport_soap.ToString().Split(';');
                foreach (string ws_importationUrl in liste_ws_importation)
                {
                    try
                    {
                        log.Info("AVANT APPEL ws_import à l'adresse[" + ws_importationUrl + "]");
                        ws_importation.wsimport ws_imp = new ws_importation.wsimport();
                        ws_imp.Url = ws_importationUrl;
                        log.Info("RETOUR : " + ws_imp.TacheImport(""));
                        //ws_imp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }
        }



        // Set up a call-back function that is invoked by the proxy class
        public void TacheImportCompleted(object sender, ws_importation.TacheImportCompletedEventArgs args)
        {
            log.Debug("Retour Appel asynchrone terminé : " + args.Result);

        }

        private void ExecutionAsynch(object source, ElapsedEventArgs e)
        {
            doCallWsImportationAsynch();
        }

        private void doCallWsImportationAsynch()
        {



            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.liste_wsimport_soap.ToString()))
            {
                string[] liste_ws_importation = sfwServiceTache.Properties.Settings.Default.liste_wsimport_soap.ToString().Split(';');
                foreach (string ws_importationUrl in liste_ws_importation)
                {
                    try
                    {
                        log.Info("AVANT APPEL ws_import à l'adresse[" + ws_importationUrl + "]");
                        ws_imp = new ws_importation.wsimport();
                        ws_imp.Url = ws_importationUrl;
                        log.Info("DEFAULT TIMEOUT Wsimportation" + ws_imp.Timeout.ToString());
                        // ws_imp.TacheImportCompleted += new ws_importation.TacheImportCompletedEventHandler(this.TacheImportCompleted);
                        object userState = "";
                        ws_imp.TacheImportAsync("", userState);
                        log.Info("RETOUR : " + (string)userState);
                        //ws_imp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToString());
                    }
                }
            }

        }

        private void timerRapideCode(object source, ElapsedEventArgs e)
        {
            log.Info("IimerRapide 2s Tick !");
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.sfwServiceTache_WSZCL00_WSZCL00.ToString()))
            {
                string[] wszcl00UrlList = sfwServiceTache.Properties.Settings.Default.sfwServiceTache_WSZCL00_WSZCL00.ToString().Split(';');
                //string wszcl00Url = sfwServiceTache.Properties.Settings.Default.sfwServiceTache_WSZCL00_WSZCL00.ToString();
                foreach (string wszcl00Url in wszcl00UrlList)
                {

                    string nomMachine = "";
                    try
                    {
                        nomMachine = Environment.MachineName;
                    }
                    catch (Exception exp)
                    {
                        log.Error("nomMachine " + exp.Message);
                    }

                    timerRapide.Stop();
                    try
                    {
                        WSZCL00.WSZCL00 wszcl00 = new WSZCL00.WSZCL00();
                        wszcl00.Url = wszcl00Url;
                        if ((!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.wsUsername)) && (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.wsPassword)))
                        {
                            WSZCL00.Authentication header = new WSZCL00.Authentication();

                            header.Username = SafeWare.Chiffrement.Dechiffre(sfwServiceTache.Properties.Settings.Default.wsUsername, SafeWare.Chiffrement.password); //
                            //sfwServiceTache.Properties.Settings.Default.wsUsername; //"toto";
                            if (header.Username == "")
                            {
                                header.Username = SafeWare.Chiffrement.DechiffreOld(sfwServiceTache.Properties.Settings.Default.wsUsername, SafeWare.Chiffrement.password); //
                                header.Password = SafeWare.Chiffrement.DechiffreOld(sfwServiceTache.Properties.Settings.Default.wsPassword, SafeWare.Chiffrement.password); // "toto";
                            }
                            else
                            {
                                header.Username = SafeWare.Chiffrement.Dechiffre(sfwServiceTache.Properties.Settings.Default.wsUsername, SafeWare.Chiffrement.password); //
                                header.Password = SafeWare.Chiffrement.Dechiffre(sfwServiceTache.Properties.Settings.Default.wsPassword, SafeWare.Chiffrement.password); // "toto";
                            }

                            log.Info("**** AFTER SET HEADER [" + header.Username + ":" + header.Password + "}");

                            wszcl00.AuthenticationValue = header;
                        }



                        wszcl00.copieFichier(nomMachine);
                        log.Debug("Telem Access copieFichier !");
                    }
                    catch (Exception exp)
                    {
                        log.Error(exp.Message);
                    }
                    timerRapide.Start();
                }
            }
        }

        private void timerMinuteCode(object source, ElapsedEventArgs e)
        {
            log.Info("Iimer1 Tick !");
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.sfwServiceTache_WSZCL00_WSZCL00.ToString()))
            {
                string[] wszcl00UrlList = sfwServiceTache.Properties.Settings.Default.sfwServiceTache_WSZCL00_WSZCL00.ToString().Split(';');
                //string wszcl00Url = sfwServiceTache.Properties.Settings.Default.sfwServiceTache_WSZCL00_WSZCL00.ToString();
                foreach (string wszcl00Url in wszcl00UrlList)
                {


                    string nomMachine = "";
                    try
                    {
                        nomMachine = Environment.MachineName;
                    }
                    catch (Exception exp)
                    {
                        log.Error("nomMachine " + exp.Message);
                    }

                    timerMinute.Stop();
                    try
                    {

                        WSZCL00.WSZCL00 wszcl00 = new WSZCL00.WSZCL00();
                        wszcl00.Url = wszcl00Url;
                        if ((!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.wsUsername)) && (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.wsPassword)))
                        {
                            WSZCL00.Authentication header = new WSZCL00.Authentication();

                            header.Username = SafeWare.Chiffrement.Dechiffre(sfwServiceTache.Properties.Settings.Default.wsUsername, SafeWare.Chiffrement.password); //
                            //sfwServiceTache.Properties.Settings.Default.wsUsername; //"toto";
                            if (header.Username == "")
                            {
                                header.Username = SafeWare.Chiffrement.DechiffreOld(sfwServiceTache.Properties.Settings.Default.wsUsername, SafeWare.Chiffrement.password); //
                                header.Password = SafeWare.Chiffrement.DechiffreOld(sfwServiceTache.Properties.Settings.Default.wsPassword, SafeWare.Chiffrement.password); // "toto";
                            }
                            else
                            {
                                header.Username = SafeWare.Chiffrement.Dechiffre(sfwServiceTache.Properties.Settings.Default.wsUsername, SafeWare.Chiffrement.password); //
                                header.Password = SafeWare.Chiffrement.Dechiffre(sfwServiceTache.Properties.Settings.Default.wsPassword, SafeWare.Chiffrement.password); // "toto";
                            }

                            wszcl00.AuthenticationValue = header;
                        }

                        wszcl00.copieFichier(nomMachine);
                        log.Debug("Telem Access copieFichier !");

                        // Liste des sites ayant besoin d'un action pour ce poste
                        // boucle sur les listes  
                        foreach (string paramSite in wszcl00.getSitesNonTraite(nomMachine))
                        {
                            log.Info("**** getSitesTraiteNow [" + paramSite + "}");
                            wszcl00.getSitesTraiteNow(paramSite);
                        }

                        wszcl00.verifierSortiesCTRL(nomMachine);
                        log.Debug("Telem Access sorties ctrl !");
                        wszcl00.synchro(nomMachine);
                        log.Debug("Telem Access synchro !");
                        wszcl00.entrerLmp(nomMachine);
                        log.Debug("Telem Access LPM !");

                    }
                    catch (Exception exp)
                    {
                        log.Error(exp.Message);
                    }
                    timerMinute.Start();
                    log.Debug("Iimer1 Tick ! End");
                }
            }

        }



        private string getPostwww(string uri, string str)
        {
            string retour = "";


            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.KeepAlive = false;
                //request.Accept = "application/xml";
                request.Method = "POST";
                try
                {
                    request.Timeout = int.Parse("10000");
                }
                catch (Exception ex)
                {
                    request.Timeout = 10000;
                    log.Error("LECTURE TIMEOUT PAR DEFAUT : " + ex.Message);

                }

                byte[] postBytes = Encoding.ASCII.GetBytes(str);
                //request.ContentType = "application/xml";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;

                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                retour = new StreamReader(response.GetResponseStream()).ReadToEnd();

                log.Debug("Retour ws : [" + response.StatusCode.ToString() + ";" + response.StatusDescription + ";" + retour + "]");
            }
            catch (Exception ex)
            {
                retour = "-1:";

                log.Error("Retour : [" + uri + "/" + str + "/" + ex.ToString() + "]");
            }

            return retour;

        }
        void StartWebRequest(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                // request.KeepAlive = false;

                request.Method = "GET";
                try
                {
                    request.Timeout = int.Parse("10000");
                }
                catch (Exception ex)
                {
                    request.Timeout = 10000;
                    log.Error("LECTURE TIMEOUT PAR DEFAUT : " + ex.Message);

                }

                request.BeginGetResponse(new AsyncCallback(FinishWebRequest), request);
            }
            catch (Exception ex)
            {

                log.Error("Retour : [" + uri + "/" + ex.ToString() + "]");
            }



        }

        void FinishWebRequest(IAsyncResult result)
        {
            log.Debug("BEFORE FinishWebRequest");

            HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
            retourGlb = new StreamReader(response.GetResponseStream()).ReadToEnd();
            log.Debug("Retour FinishWebRequest : [" + response.StatusCode.ToString() + ";" + response.StatusDescription + ";" + retourGlb + "]");
        }

        private string getwwwAsync(string uri)
        {
            log.Debug("BEFORE getwwwAsync");
            StartWebRequest(uri);
            log.Debug("AFTER StartWebRequest");
            return retourGlb;
        }

        private string getwww(string uri)
        {
            string retour = "";


            try
            {
                log.Debug("request from uri[" + uri + "]");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.KeepAlive = false;

                request.Method = "GET";
                try
                {
                    request.Timeout = int.Parse("1000000");
                }
                catch (Exception ex)
                {
                    request.Timeout = 10000;
                    log.Error("LECTURE TIMEOUT PAR DEFAUT : " + ex.Message);

                }


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                retour = new StreamReader(response.GetResponseStream()).ReadToEnd();

                log.Debug("Retour ws : [" + response.StatusCode.ToString() + ";" + response.StatusDescription + ";" + retour + "]");
            }
            catch (Exception ex)
            {
                retour = "-1:";

                log.Error("Retour : [" + uri + "/" + ex.ToString() + "]");
            }

            return retour;

        }

        public string getResponseFromAWs(string url, string str)
        {

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-Fr");
            //string url = siteXml.SelectSingleNode("URL_REMOTE").InnerText + "/listeProfils";
            // string url = @ZCL40ESVisiteApp.Properties.Settings.Default.URLESVISITE + "/entreeSortieBadge";



            StartWebRequest(url);   // GET
                                    //string retour = getPostwww(url, str);
            string retour = "<?xml version=\"1.0\"?><string>OK</string>";
            log.Info(retour);

            try
            {
                XmlDocument _xmlResponse = new XmlDocument();
                _xmlResponse.LoadXml(retour);
                int i = 0;
                if (_xmlResponse.FirstChild.Name.Equals("xml"))
                {
                    _xmlResponse.RemoveChild(_xmlResponse.FirstChild);
                }
                List<string> _listeRetour = new List<string>();
                log.Info("xmlReal [" + _xmlResponse.InnerXml + "]");
                foreach (XmlNode _node in _xmlResponse.ChildNodes)
                {
                    if (_node.Name.Equals("string"))
                    {

                        _listeRetour.Add(_node.InnerText);
                        log.Debug("listeRetour[" + i.ToString() + "]=(" + _listeRetour[i].ToString() + ")");
                        i++;

                    }

                }

                string _final = _listeRetour.Count > 1 ? _listeRetour[0].ToString() + "|" + _listeRetour[1].ToString() : _listeRetour[0].ToString();
                return _final;
            }
            catch (Exception ex)
            {
                log.Error(" No ArrayOfString :" + retour + ": " + ex.ToString());
                return "-1";
            }
        }

        public string getResponseAWs(string url)
        {

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-Fr");
            //string url = siteXml.SelectSingleNode("URL_REMOTE").InnerText + "/listeProfils";
            // string url = @ZCL40ESVisiteApp.Properties.Settings.Default.URLESVISITE + "/entreeSortieBadge";




            string retour = getwww(url);

            log.Info(retour);

            try
            {
                XmlDocument _xmlResponse = new XmlDocument();
                _xmlResponse.LoadXml(retour);
                int i = 0;
                if (_xmlResponse.FirstChild.Name.Equals("xml"))
                {
                    _xmlResponse.RemoveChild(_xmlResponse.FirstChild);
                }
                List<string> _listeRetour = new List<string>();
                log.Info("xmlReal [" + _xmlResponse.InnerXml + "]");
                foreach (XmlNode _node in _xmlResponse.ChildNodes)
                {
                    if (_node.Name.Equals("string"))
                    {

                        _listeRetour.Add(_node.InnerText);
                        log.Debug("listeRetour[" + i.ToString() + "]=(" + _listeRetour[i].ToString() + ")");
                        i++;

                    }

                }

                string _final = _listeRetour.Count > 1 ? _listeRetour[0].ToString() + "|" + _listeRetour[1].ToString() : _listeRetour[0].ToString();
                return _final;
            }
            catch (Exception ex)
            {
                log.Error(" No ArrayOfString :" + retour + ": " + ex.Message);
                return "-1";
            }
        }

        private string appelWebClient(string url, string autoValidation)
        {
            string Retour = "";
            try
            {
                if (autoValidation == "1")
                {
                    // Auto validation de la confirmation du certificat SSL
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    log.Debug("autoValidation");
                }
                // Appel du lien http
                WebClient wc = new WebClient();
                Retour = wc.DownloadString(new Uri(url));
                wc.Dispose();
            }
            catch (Exception ex)
            {
                // Affiche l'exception dans la zone de résultat.
                log.Error("appelWebClient [" + ex.ToString() + "]");
                Retour = "ERROR";
            }
            return Retour;
        }

        private void ExecOmni(object source, ElapsedEventArgs e)
        {
            doCallWsOmni();
        }

        private void doCallWsOmni()
        {
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.listeAppelsTimerOmni.ToString()))
            {
                string[] liste_ws = sfwServiceTache.Properties.Settings.Default.listeAppelsTimerOmni.ToString().Split(';');
                foreach (string wsUrl in liste_ws)
                {
                    try
                    {

                        log.Info("AVANT APPEL ws_ à l'adresse[" + wsUrl + "]");

                        log.Info("RETOUR ws " + getResponseFromAWs(wsUrl, ""));
                        //ws_imp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }
        }


        private void ExecCheckTasks(object source, ElapsedEventArgs e)
        {
            doCallCheckTasks();



        }



        private void ExecGetCalls(object source, ElapsedEventArgs e)
        {
            doExecGetCalls();



        }

        private void doExecGetCalls()
        {
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.listeAppelsTimerGet.ToString()))
            {
                string[] liste_ws = sfwServiceTache.Properties.Settings.Default.listeAppelsTimerGet.ToString().Split(';');
                foreach (string wsUrl in liste_ws)
                {
                    try
                    {
                        log.Info("AVANT APPEL ws_ à l'adresse[" + wsUrl + "]");
                        log.Info("retour APPEL doExecGetCalls" + getwwwAsync(wsUrl) + "]");
                        //   log.Info("RETOUR ws " + getResponseAWs(wsUrl));

                        //ws_imp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }
        }
        private void doCallCheckTasks()
        {
            Thread.Sleep(1000);
            calculNextTick();
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.listeAppelsHeureFixe.ToString()))
            {
                string[] liste_ws = sfwServiceTache.Properties.Settings.Default.listeAppelsHeureFixe.ToString().Split(';');
                foreach (string wsUrl in liste_ws)
                {
                    try
                    {
                        log.Info("AVANT APPEL ws_ à l'adresse[" + wsUrl + "]");

                        log.Info("RETOUR ws " + getResponseAWs(wsUrl));

                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }


        }

        private void ExecDynCalls(object source, ElapsedEventArgs e)
        {
            doExecDynCalls();



        }

        private void doExecDynCalls()
        {
            if (!String.IsNullOrEmpty(sfwServiceTache.Properties.Settings.Default.listeAppelsTimerDyn.ToString()))
            {
                string[] liste_ws = sfwServiceTache.Properties.Settings.Default.listeAppelsTimerDyn.ToString().Split(';');
                foreach (string wsUrl in liste_ws)
                {
                    try
                    {
                        log.Info("AVANT APPEL ws_ à l'adresse[" + wsUrl + "]");
                        log.Info("retour APPEL doExecGetCalls" + getwww(wsUrl) + "]");
                        //   log.Info("RETOUR ws " + getResponseAWs(wsUrl));

                        //ws_imp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }
        }


    }
}
