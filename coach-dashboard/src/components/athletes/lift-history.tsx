'use client';

import { useState } from 'react';
import { useMaxLifts, useExerciseHistory } from '@/hooks/athletes/use-athletes';
import { 
  LineChart, 
  Line, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  ReferenceLine
} from 'recharts';
import { 
  TrendingUp, 
  Dumbbell, 
  Award,
  Activity
} from 'lucide-react';
import { format } from 'date-fns';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

interface LiftHistoryProps {
  athleteId: string;
}

export function AthleteLiftHistory({ athleteId }: LiftHistoryProps) {
  const { data: maxLiftsData, isLoading: isLoadingMaxLifts } = useMaxLifts(athleteId);
  const [selectedExerciseId, setSelectedExerciseId] = useState<string>('');

  // Fallback to first exercise if none is selected
  if (maxLiftsData?.maxLifts?.length && !selectedExerciseId) {
    setSelectedExerciseId(maxLiftsData.maxLifts[0].exerciseId);
  }

  const { data: exerciseHistory, isLoading: isLoadingHistory } = useExerciseHistory(
    athleteId, 
    selectedExerciseId,
  );

  // Parse chart data
  const chartData = [...(exerciseHistory?.recentLogs || [])]
    .sort((a, b) => new Date(a.performedAt).getTime() - new Date(b.performedAt).getTime())
    .map(log => ({
      date: format(new Date(log.performedAt), 'MMM d'),
      fullDate: format(new Date(log.performedAt), 'MMM d, yyyy'),
      weight: log.weight,
      details: `${log.sets}x${log.reps} @ ${log.weight}kg`,
    }));

  return (
    <Card className="mt-8 shadow-sm">
      <CardHeader className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b">
        <div>
          <CardTitle className="text-xl flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-blue-600" />
            Lift History & PRs
          </CardTitle>
          <p className="text-sm text-slate-500 mt-1">Track strength progression over time.</p>
        </div>
        
        <div className="w-full sm:w-64 mt-4 sm:mt-0">
          <Select value={selectedExerciseId} onValueChange={setSelectedExerciseId}>
            <SelectTrigger>
              <SelectValue placeholder="Select an exercise" />
            </SelectTrigger>
            <SelectContent>
              {isLoadingMaxLifts ? (
                <SelectItem value="loading" disabled>Loading exercises...</SelectItem>
              ) : maxLiftsData?.maxLifts?.length === 0 ? (
                <SelectItem value="none" disabled>No max lifts recorded</SelectItem>
              ) : (
                maxLiftsData?.maxLifts?.map((lift) => (
                  <SelectItem key={lift.exerciseId} value={lift.exerciseId}>
                    {lift.exerciseName}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </div>
      </CardHeader>
      
      <CardContent className="p-6">
        {selectedExerciseId ? (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            
            {/* PR Details Section */}
            <div className="col-span-1 space-y-6">
              {/* CURRENT 1RM CARD */}
              <div className="p-6 bg-gradient-to-br from-blue-600 to-indigo-700 rounded-xl text-white shadow-md relative overflow-hidden">
                <Dumbbell className="absolute -right-4 -bottom-4 w-32 h-32 text-white/10" />
                <h3 className="font-medium text-blue-100 flex items-center gap-2">
                  <Award className="w-4 h-4" /> Current Estimated 1RM
                </h3>
                <div className="mt-4 flex items-end gap-2 relative z-10">
                  <span className="text-5xl font-bold tracking-tight">
                    {exerciseHistory?.current1RM?.weight || '--'}
                  </span>
                  <span className="text-blue-200 pb-1.5 text-lg">kg</span>
                </div>
                {exerciseHistory?.current1RM?.recordedAt && (
                   <p className="mt-4 text-xs text-blue-200 bg-black/20 px-3 py-1.5 rounded inline-block font-medium">
                     Tested: {format(new Date(exerciseHistory.current1RM.recordedAt), 'MMM d, yyyy')}
                   </p>
                )}
              </div>

              {/* LATEST LOGS LIST */}
              <div>
                <h3 className="font-semibold text-slate-800 mb-4 flex items-center gap-2 text-sm uppercase tracking-wider">
                  <Activity className="w-4 h-4 text-slate-500" /> Recent Performances
                </h3>
                {isLoadingHistory ? (
                  <p className="text-sm text-slate-500">Loading history...</p>
                ) : (!exerciseHistory || exerciseHistory.recentLogs.length === 0) ? (
                  <div className="p-6 bg-slate-50 text-center rounded-xl border border-slate-100 border-dashed">
                    <p className="text-sm text-slate-500">No logs recorded for this exercise yet.</p>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {exerciseHistory.recentLogs.slice(0, 5).map((log, i) => (
                      <div key={i} className="flex justify-between items-center p-3.5 rounded-xl bg-slate-50 border border-slate-100 hover:bg-slate-100 transition-colors">
                        <div>
                          <p className="font-semibold text-slate-900 text-lg leading-none">{log.weight} kg</p>
                          <p className="text-sm text-slate-500 mt-1">{log.sets} sets × {log.reps} reps</p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm font-medium text-slate-600">{format(new Date(log.performedAt), 'MMM d')}</p>
                          {log.rpe && <p className="text-xs text-slate-400 mt-0.5 font-medium">RPE {log.rpe}</p>}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>

            {/* CHART SECTION */}
            <div className="col-span-1 lg:col-span-2 min-h-[400px] flex flex-col p-4 bg-slate-50/50 rounded-2xl border border-slate-100">
              <h3 className="font-semibold text-slate-800 mb-6 text-sm uppercase tracking-wider px-2">Weight Progression (Top Sets)</h3>
              
              {chartData.length > 0 ? (
                <div className="flex-1 w-full relative min-h-[350px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={chartData} margin={{ top: 10, right: 30, bottom: 10, left: 10 }}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                      <XAxis 
                        dataKey="date" 
                        axisLine={false} 
                        tickLine={false} 
                        tick={{ fill: '#64748B', fontSize: 13, fontWeight: 500 }}
                        dy={15}
                      />
                      <YAxis 
                        axisLine={false} 
                        tickLine={false} 
                        tick={{ fill: '#64748B', fontSize: 13, fontWeight: 500 }}
                        domain={['auto', 'auto']}
                        dx={-15}
                      />
                      <Tooltip 
                        contentStyle={{ 
                          borderRadius: '12px', 
                          border: 'none', 
                          boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
                          padding: '12px 16px'
                        }}
                        labelStyle={{ color: '#64748B', fontWeight: 600, marginBottom: '6px', fontSize: '13px' }}
                        itemStyle={{ color: '#0F172A', fontWeight: 700, fontSize: '16px' }}
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        formatter={(value: any, _name: any, props: any) => [
                          `${value} kg (${props.payload.details})`, 
                          'Top Set'
                        ]}
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        labelFormatter={(label: any, payload: any) => payload?.[0]?.payload?.fullDate || label}
                      />
                      
                      {/* PR Reference Line if available */}
                      {exerciseHistory?.personalRecord && (
                        <ReferenceLine 
                          y={exerciseHistory.personalRecord.weight} 
                          stroke="#F59E0B" 
                          strokeDasharray="4 4" 
                          strokeWidth={2}
                          label={{ 
                            position: 'insideTopLeft', 
                            value: 'All-Time PR', 
                            fill: '#D97706', 
                            fontSize: 13, 
                            fontWeight: 600,
                            dy: -10 
                          }} 
                        />
                      )}

                      <Line 
                        type="monotone" 
                        dataKey="weight" 
                        stroke="#2563EB" 
                        strokeWidth={4}
                        dot={{ r: 5, strokeWidth: 3, fill: '#FFFFFF' }}
                        activeDot={{ r: 8, fill: '#2563EB', stroke: '#FFFFFF', strokeWidth: 3 }}
                        animationDuration={1500}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              ) : (
                <div className="flex-1 flex flex-col items-center justify-center text-slate-400">
                  <TrendingUp className="w-16 h-16 mb-4 opacity-20" />
                  <p className="font-medium text-slate-500">Not enough data to display a chart.</p>
                  <p className="text-sm mt-1 text-slate-400">Start logging this exercise to see a progression track.</p>
                </div>
              )}
            </div>

          </div>
        ) : (
           <div className="py-16 border-2 border-dashed border-slate-200 rounded-2xl text-center bg-slate-50/50">
             <Dumbbell className="w-12 h-12 text-slate-300 mx-auto mb-4" />
             <h3 className="text-lg font-semibold text-slate-900">Select an Exercise</h3>
             <p className="text-slate-500 mt-2 max-w-md mx-auto">Please choose an exercise from the dropdown above to view the athlete&apos;s progression history, PRs, and recent performance.</p>
           </div>
        )}
      </CardContent>
    </Card>
  );
}