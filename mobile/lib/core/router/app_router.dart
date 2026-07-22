import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../features/auth/presentation/pages/login_page.dart';
import '../../features/auth/presentation/pages/register_page.dart';
import '../../features/auth/presentation/pages/splash_page.dart';
import '../../features/capture/presentation/bloc/capture_bloc.dart';
import '../../features/capture/presentation/pages/manual_capture_page.dart';
import '../../features/capture/presentation/pages/ocr_capture_page.dart';
import '../../features/capture/presentation/pages/qr_capture_page.dart';
import '../../features/capture/presentation/pages/voice_capture_page.dart';
import '../../features/dashboard/presentation/bloc/dashboard_bloc.dart';
import '../../features/dashboard/presentation/pages/dashboard_page.dart';
import '../../features/transactions/presentation/bloc/transaction_bloc.dart';
import '../../features/transactions/presentation/pages/add_transaction_page.dart';
import '../../features/transactions/presentation/pages/transaction_detail_page.dart';
import '../di/injection_container.dart';
import '../network/api_client.dart';

class AppRouter {
  static final GoRouter router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(
        path: '/',
        builder: (context, state) => const SplashPage(),
      ),
      GoRoute(
        path: '/login',
        builder: (context, state) => BlocProvider(
          create: (_) => sl<AuthBloc>(),
          child: const LoginPage(),
        ),
      ),
      GoRoute(
        path: '/register',
        builder: (context, state) => BlocProvider(
          create: (_) => sl<AuthBloc>(),
          child: const RegisterPage(),
        ),
      ),
      ShellRoute(
        builder: (context, state, child) => MultiBlocProvider(
          providers: [
            BlocProvider(create: (_) => sl<DashboardBloc>()),
            BlocProvider(create: (_) => sl<TransactionBloc>()),
          ],
          child: child,
        ),
        routes: [
          GoRoute(
            path: '/dashboard',
            builder: (context, state) => const DashboardPage(),
          ),
          GoRoute(
            path: '/transactions/add',
            builder: (context, state) => const AddTransactionPage(),
          ),
          GoRoute(
            path: '/transactions/:id',
            builder: (context, state) => TransactionDetailPage(
              transactionId: state.pathParameters['id']!,
            ),
          ),
        ],
      ),
      // ── Capture Routes (each owns its own CaptureBloc instance) ──
      GoRoute(
        path: '/capture/voice',
        builder: (context, state) => BlocProvider(
          create: (_) => CaptureBloc(sl<ApiClient>()),
          child: const VoiceCapturePage(),
        ),
      ),
      GoRoute(
        path: '/capture/ocr',
        builder: (context, state) => BlocProvider(
          create: (_) => CaptureBloc(sl<ApiClient>()),
          child: const OcrCapturePage(),
        ),
      ),
      GoRoute(
        path: '/capture/qr',
        builder: (context, state) => BlocProvider(
          create: (_) => CaptureBloc(sl<ApiClient>()),
          child: const QrCapturePage(),
        ),
      ),
      GoRoute(
        path: '/capture/manual',
        builder: (context, state) => BlocProvider(
          create: (_) => CaptureBloc(sl<ApiClient>()),
          child: const ManualCapturePage(),
        ),
      ),
    ],
  );
}
