﻿using System.Web;
using System.Web.Optimization;

namespace LBCAlerterWeb
{
    public class BundleConfig
    {
        // Pour plus d’informations sur le regroupement, rendez-vous sur http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilisez la version de développement de Modernizr pour développer et apprendre. Puis, lorsque vous êtes
            // prêt pour la production, utilisez l’outil de génération sur http://modernizr.com pour sélectionner uniquement les tests dont vous avez besoin.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/minimal").Include(
                        "~/Scripts/jquery.mmenu.*",
                        "~/Scripts/jquery.sparkline.js",
                        "~/Scripts/jquery.nicescroll.js",
                        "~/Scripts/jquery.animateNumbers.js",
                        "~/Scripts/jquery.videobackground.js",
                        "~/Scripts/bootstrap-datepicker.js",                 
                        "~/Scripts/minimal.js"));

            bundles.Add(new StyleBundle("~/Content/jqueryui").Include(
                        "~/Content/themes/base/*.css"));

            bundles.Add(new StyleBundle("~/Content/minimal").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/font-awesome.css",
                        "~/Content/jquery.mmenu.*",
                        "~/Scripts/jquery.datepicker.css",
                        "~/Content/minimal.css",
                        "~/Content/pretty.css"));
        }
    }
}
