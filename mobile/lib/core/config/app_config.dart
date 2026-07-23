class AppConfig {
  static const String appName = 'FundWise AI';
  static const String appVersion = '1.0.0 (Phase 1 MVP)';
  
  // Base API URL — change to production backend URL when deploying
  static const String baseUrl = 'http://localhost:5207/api';
  static const String defaultCurrency = 'EGP';
  
  static const Duration requestTimeout = Duration(seconds: 30);
}

class ApiEndpoints {
  static const String register = '/auth/register';
  static const String login = '/auth/login';
  static const String refresh = '/auth/refresh';
  static const String logout = '/auth/logout';
  static const String me = '/auth/me';
  
  static const String transactions = '/transactions';
  static const String voiceCapture = '/voice/capture';
  static const String ocrReceipt = '/ocr/receipt';
  static const String smsParse = '/sms/parse';
  static const String qrParse = '/qr/parse';
  
  static const String dashboardSummary = '/dashboard/summary';
  static const String recommendations = '/recommendations';
}
