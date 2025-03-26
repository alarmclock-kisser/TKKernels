__kernel void test_exp(__global int* data, const int size){
    int gid = get_global_id(0);
    if (gid < size) {
        data[gid] *= data[gid];
    }
}