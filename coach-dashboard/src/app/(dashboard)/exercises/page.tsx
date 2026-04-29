'use client';

import { useState } from 'react';
import { PageHeader, DataTable, Column } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Plus, Search, Pencil, Trash2 } from 'lucide-react';
import { useExercises, useCreateExercise, useUpdateExercise, useDeleteExercise } from '@/hooks';
import {
  ExerciseListItem,
  ExerciseCategory,
  MuscleGroup,
  CreateExerciseRequest,
  UpdateExerciseRequest,
} from '@/types';

const CATEGORIES: ExerciseCategory[] = [
  'Squat', 'Bench', 'Deadlift', 'OverheadPress',
  'Row', 'Pull', 'Accessory', 'Cardio', 'Mobility', 'Core',
];

const MUSCLE_GROUPS: MuscleGroup[] = [
  'Chest', 'Back', 'Shoulders', 'Biceps', 'Triceps',
  'Forearms', 'Quads', 'Hamstrings', 'Glutes', 'Calves',
  'Core', 'Traps', 'FullBody',
];

type FormData = {
  name: string;
  description: string;
  category: ExerciseCategory;
  primaryMuscleGroup: MuscleGroup;
  videoUrl: string;
  equipment: string;
  isCompound: boolean;
};

const emptyForm: FormData = {
  name: '',
  description: '',
  category: 'Squat',
  primaryMuscleGroup: 'Quads',
  videoUrl: '',
  equipment: '',
  isCompound: true,
};

export default function ExercisesPage() {
  const [category, setCategory] = useState<string>('');
  const [muscleGroup, setMuscleGroup] = useState<string>('');
  const [search, setSearch] = useState('');

  // CRUD hooks
  const { data, isLoading } = useExercises({
    category: category && category !== 'all' ? category : undefined,
    muscleGroup: muscleGroup && muscleGroup !== 'all' ? muscleGroup : undefined,
  });
  const createExercise = useCreateExercise();
  const updateExercise = useUpdateExercise();
  const deleteExercise = useDeleteExercise();

  // Dialog state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<FormData>(emptyForm);

  const filteredData = data?.filter((exercise) =>
    exercise.name.toLowerCase().includes(search.toLowerCase())
  ) || [];

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setDialogOpen(true);
  };

  const openEdit = (row: ExerciseListItem) => {
    setEditingId(row.id);
    setForm({
      name: row.name,
      description: '',
      category: row.category,
      primaryMuscleGroup: row.primaryMuscleGroup,
      videoUrl: '',
      equipment: row.equipment || '',
      isCompound: row.isCompound,
    });
    setDialogOpen(true);
  };

  const handleSubmit = async () => {
    if (!form.name.trim()) return;

    if (editingId) {
      const payload: UpdateExerciseRequest = {
        name: form.name,
        description: form.description || undefined,
        category: form.category,
        primaryMuscleGroup: form.primaryMuscleGroup,
        videoUrl: form.videoUrl || undefined,
        equipment: form.equipment || undefined,
        isCompound: form.isCompound,
      };
      await updateExercise.mutateAsync({ exerciseId: editingId, data: payload });
    } else {
      const payload: CreateExerciseRequest = {
        name: form.name,
        description: form.description || undefined,
        category: form.category,
        primaryMuscleGroup: form.primaryMuscleGroup,
        videoUrl: form.videoUrl || undefined,
        equipment: form.equipment || undefined,
        isCompound: form.isCompound,
      };
      await createExercise.mutateAsync(payload);
    }
    setDialogOpen(false);
  };

  const handleDelete = async (row: ExerciseListItem) => {
    if (!confirm(`¿Desactivar "${row.name}"?`)) return;
    await deleteExercise.mutateAsync(row.id);
  };

  const columns: Column<ExerciseListItem>[] = [
    {
      key: 'name',
      header: 'Exercise Name',
      cell: (row) => (
        <div>
          <p className="font-medium text-slate-900">{row.name}</p>
          {row.equipment && (
            <p className="text-sm text-slate-500 truncate max-w-xs">{row.equipment}</p>
          )}
        </div>
      ),
    },
    {
      key: 'category',
      header: 'Category',
      cell: (row) => <Badge variant="outline">{row.category}</Badge>,
    },
    {
      key: 'muscleGroup',
      header: 'Muscle Group',
      cell: (row) => <span className="text-slate-600">{row.primaryMuscleGroup}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      cell: (row) => (
        <Badge variant={row.isActive ? 'default' : 'secondary'}>
          {row.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      cell: (row) => (
        <div className="flex gap-1 justify-end">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); openEdit(row); }}>
            <Pencil className="h-4 w-4" />
          </Button>
          {row.isActive && (
            <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); handleDelete(row); }}>
              <Trash2 className="h-4 w-4 text-red-500" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Exercise Library"
        description="Manage your exercise library and templates"
        actions={
          <Button onClick={openCreate}>
            <Plus className="w-4 h-4 mr-2" />
            Add Exercise
          </Button>
        }
      />

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <Input
            type="search"
            placeholder="Search exercises..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-10"
          />
        </div>
        <Select value={category} onValueChange={setCategory}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="All categories" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All categories</SelectItem>
            {CATEGORIES.map((c) => (
              <SelectItem key={c} value={c}>{c}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={muscleGroup} onValueChange={setMuscleGroup}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="All muscle groups" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All muscle groups</SelectItem>
            {MUSCLE_GROUPS.map((mg) => (
              <SelectItem key={mg} value={mg}>{mg}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <DataTable
        columns={columns}
        data={filteredData}
        isLoading={isLoading}
        emptyMessage="No exercises found. Add your first exercise!"
        rowKey={(row) => row.id}
      />

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>{editingId ? 'Edit Exercise' : 'New Exercise'}</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="e.g. Back Squat"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description</Label>
              <Input
                id="description"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="Optional description"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label>Category *</Label>
                <Select value={form.category} onValueChange={(v) => setForm({ ...form, category: v as ExerciseCategory })}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {CATEGORIES.map((c) => (
                      <SelectItem key={c} value={c}>{c}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid gap-2">
                <Label>Muscle Group *</Label>
                <Select value={form.primaryMuscleGroup} onValueChange={(v) => setForm({ ...form, primaryMuscleGroup: v as MuscleGroup })}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {MUSCLE_GROUPS.map((mg) => (
                      <SelectItem key={mg} value={mg}>{mg}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="equipment">Equipment</Label>
              <Input
                id="equipment"
                value={form.equipment}
                onChange={(e) => setForm({ ...form, equipment: e.target.value })}
                placeholder="e.g. Barbell, Squat Rack"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="videoUrl">Video URL</Label>
              <Input
                id="videoUrl"
                value={form.videoUrl}
                onChange={(e) => setForm({ ...form, videoUrl: e.target.value })}
                placeholder="https://..."
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="isCompound"
                checked={form.isCompound}
                onChange={(e) => setForm({ ...form, isCompound: e.target.checked })}
                className="h-4 w-4"
              />
              <Label htmlFor="isCompound">Compound exercise</Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              onClick={handleSubmit}
              disabled={!form.name.trim() || createExercise.isPending || updateExercise.isPending}
            >
              {createExercise.isPending || updateExercise.isPending ? 'Saving...' : editingId ? 'Update' : 'Create'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
