import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../domain/entities/dashboard_entities.dart';
import '../../domain/repositories/dashboard_repository.dart';

// ─── Events ──────────────────────────────────────────────────────────────
sealed class DashboardEvent extends Equatable {
  @override List<Object?> get props => [];
}

class DashboardLoadRequested extends DashboardEvent {
  final int? month;
  final int? year;
  DashboardLoadRequested({this.month, this.year});
  @override List<Object?> get props => [month, year];
}

class DashboardRefreshRequested extends DashboardEvent {}

// ─── States ──────────────────────────────────────────────────────────────
sealed class DashboardState extends Equatable {
  @override List<Object?> get props => [];
}

class DashboardInitial extends DashboardState {}
class DashboardLoading extends DashboardState {}
class DashboardLoaded extends DashboardState {
  final DashboardSummary summary;
  DashboardLoaded(this.summary);
  @override List<Object?> get props => [summary];
}
class DashboardError extends DashboardState {
  final String message;
  DashboardError(this.message);
  @override List<Object?> get props => [message];
}

// ─── BLoC ─────────────────────────────────────────────────────────────────
class DashboardBloc extends Bloc<DashboardEvent, DashboardState> {
  final GetDashboardSummaryUsecase getDashboardSummary;

  DashboardBloc({required this.getDashboardSummary}) : super(DashboardInitial()) {
    on<DashboardLoadRequested>(_onLoadRequested);
    on<DashboardRefreshRequested>((event, emit) {
      add(DashboardLoadRequested());
    });
  }

  Future<void> _onLoadRequested(DashboardLoadRequested event, Emitter<DashboardState> emit) async {
    emit(DashboardLoading());
    final result = await getDashboardSummary(DashboardParams(month: event.month, year: event.year));
    result.fold(
      (failure) => emit(DashboardError(failure.message)),
      (summary) => emit(DashboardLoaded(summary)),
    );
  }
}
