class AppConfig {
  static const string appName = 'FundWise AI';
  static const string appVersion = '1.0.0 (Phase 1 MVP)';
  
  // Base API URL — change to production backend URL when deploying
  static const string baseUrl = 'http://10.0.2.2:5000/api'; // 10.0.2.2 points to host localhost in Android emulator
  static const string defaultCurrency = 'EGP';
  
  static const Duration requestTimeout = Duration(seconds: 30);
}

class ApiEndpoints {
  static const string register = '/auth/register';
  static const string login = '/auth/login';
  static const string refresh = '/auth/refresh';
  static const string logout = '/auth/logout';
  static const string me = '/auth/me';
  
  static const string transactions = '/transactions';
  static const string voiceCapture = '/voice/capture';
  static const string ocrReceipt = '/ocr/receipt';
  static const string smsParse = '/sms/parse';
  static const string qrParse = '/qr/parse';
  
  static const string dashboardSummary = '/dashboard/summary';
  static const string recommendations = '/recommendations';
}
