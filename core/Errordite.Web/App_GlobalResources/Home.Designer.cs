//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option or rebuild the Visual Studio project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Web.Application.StronglyTypedResourceProxyBuilder", "10.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Home {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Home() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.Home", global::System.Reflection.Assembly.Load("App_GlobalResources"));
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to General comment / question.
        /// </summary>
        internal static string ContactUsReason_GeneralQuestion {
            get {
                return ResourceManager.GetString("ContactUsReason_GeneralQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Information about pricing.
        /// </summary>
        internal static string ContactUsReason_PricingInfo {
            get {
                return ResourceManager.GetString("ContactUsReason_PricingInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Report a bug.
        /// </summary>
        internal static string ContactUsReason_ReportIssue {
            get {
                return ResourceManager.GetString("ContactUsReason_ReportIssue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter your email.
        /// </summary>
        internal static string ContactUsViewModel_Email {
            get {
                return ResourceManager.GetString("ContactUsViewModel_Email", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter your message.
        /// </summary>
        internal static string ContactUsViewModel_Message {
            get {
                return ResourceManager.GetString("ContactUsViewModel_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter your name.
        /// </summary>
        internal static string ContactUsViewModel_Name {
            get {
                return ResourceManager.GetString("ContactUsViewModel_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, something went wrong and we were unable to send your message, please try again..
        /// </summary>
        internal static string MessageNotReceived {
            get {
                return ResourceManager.GetString("MessageNotReceived", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Thanks for your message, we will be in touch shortly..
        /// </summary>
        internal static string MessageReceived {
            get {
                return ResourceManager.GetString("MessageReceived", resourceCulture);
            }
        }
    }
}
